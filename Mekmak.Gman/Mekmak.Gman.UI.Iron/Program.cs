using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.PeopleService.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Mekmak.Gman.Cobalt;
using Mekmak.Gman.Diamond;
using Mekmak.Gman.Ore;
using Mekmak.Gman.Silk.Interfaces;

namespace Mekmak.Gman.UI.Iron
{
    class Program
    {
        private static ITraceLogger _log;
        private static MessageProvider _messageProvider;
        private static ContactProvider _contactProvider;
        private static LabelProvider _labelProvider;

        static void Main(string[] args)
        {
            string credentialsPath = args[0];
            string tokenDir = args[1];

            if (!File.Exists(credentialsPath) || !Directory.Exists(tokenDir))
            {
                throw new ArgumentException("Must pass in credentials file location and token directory");
            }

            _log = new ConsoleTraceLogger("Gman.UI.Iron");

            var gmailService = BuildGmailService(credentialsPath, tokenDir).Result;
            var peopleService = BuildPeopleServiceAsync(credentialsPath).Result;

            _contactProvider = new ContactProvider(_log.GetSubLogger("ContactProvider"), peopleService);
            _messageProvider = new MessageProvider(_log.GetSubLogger("MessageProvider"), gmailService);
            _labelProvider = new LabelProvider(gmailService);

            string baseTraceId = TraceId.New();
            while (true)
            {
                Console.Write("> ");
                var command = ConsoleCommand.New(Console.ReadLine());
                if (command.Current == "quit" || command.Current == "exit")
                {
                    Console.WriteLine("Goodbye!");
                    return;
                }

                try
                {
                    ProcessCommand(TraceId.New(baseTraceId), command);
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    ConsoleWriter.WriteInRed($"ERROR: {ex.Message}");
                    ConsoleWriter.WriteInRed($"{ex.StackTrace}");
                    Console.WriteLine();
                }
            }
        }

        private static async Task<PeopleServiceService> BuildPeopleServiceAsync(string credsPath)
        {
            using var stream = new FileStream(credsPath, FileMode.Open, FileAccess.Read);

            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { PeopleServiceService.Scope.Contacts },
                "user",
                CancellationToken.None,
                new FileDataStore("Mekmak.Gman.People"));

            // Create the service.
            return new PeopleServiceService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Mekmak Gman",
            });
        }

        private static async Task<GmailService> BuildGmailService(string credsPath, string tokenDir)
        {
            var factory = new GmailServiceFactory();
            return await factory.BuildAsync(new GmailServiceConfig
            {
                ApiScopes = new[]
                {
                    GmailService.Scope.GmailReadonly,
                    GmailService.Scope.GmailSend
                },
                AppName = "Mekmak Gman",
                CredentialsFilePath = credsPath,
                TokenDirectory = tokenDir
            });
        }

        private static void ProcessCommand(string traceId, ConsoleCommand command)
        {
            switch (command.Current)
            {
                case "label":
                    ProcessLabelCommand(command.Advance());
                    return;
                case "message":
                    ProcessMessageCommand(command.Advance());
                    return;
                case "contact":
                    ProcessContactCommand(traceId, command.Advance());
                    return;
                default:
                    ConsoleWriter.WriteInYellow($"Unknown command '{command}'");
                    return;
            }
        }

        private static void ProcessContactCommand(string traceId, ConsoleCommand command)
        {
            switch(command.Current)
            {
                case "search":
                    string email = command.Advance().Current;
                    if(string.IsNullOrWhiteSpace(email))
                    {
                        ConsoleWriter.WriteInYellow("Must pass email argument into contact search");
                        return;
                    }

                    var contacts = _contactProvider.SearchByEmail(traceId, email).Result;
                    foreach(var c in contacts)
                    {
                        ConsoleWriter.WriteInGreen($"\nResource name: {c.ResourceName}");
                        foreach(var n in c.Names)
                        {
                            ConsoleWriter.WriteInGreen($"\tName: {n.DisplayName}");
                        }
                    }
                    return;
                default:
                    ConsoleWriter.WriteInYellow($"Unknown contact commaind '{command}'");
                    return;
            }    
        }

        private static void ProcessLabelCommand(ConsoleCommand command)
        {
            switch (command.Current)
            {
                case "-get":
                    PrintLabels();
                    return;
                default:
                    ConsoleWriter.WriteInYellow($"Unknown label command '{command}'");
                    return;
            }
        }

        private static void ProcessMessageCommand(ConsoleCommand command)
        {
            switch (command.Current)
            {
                case "-list-with-label":
                    PrintMessages(command.Advance());
                    return;
                case "-dl-with-label":
                    DownloadWithLabel(command.Advance());
                    return;
                case "-dl":
                    DownloadWithMessageId(command.Advance());
                    return;
                case "-read":
                    ReadMessage(command.Advance());
                    return;
                case "-upload":
                    UploadReceipts(command.Advance());
                    return;
                case "-send-test":
                    SendTestMessage(command.Advance());
                    return;
                default:
                    ConsoleWriter.WriteInYellow($"Unknown message command '{command}'");
                    return;
            }
        }

        private static void SendTestMessage(ConsoleCommand command)
        {
            string photoPath = command.Current;
            ConsoleWriter.WriteInGreen($"Sending test message with photo {photoPath}...");
            string messageId = _messageProvider.SendMessage("Test Message", "Test message sent from gman app", photoPath);
            ConsoleWriter.WriteInGreen($"Sent message with id {messageId}");
        }

        private static void UploadReceipts(ConsoleCommand command)
        {
            const string sentFileName = "sent.txt";

            string photoDir = command.Current;
            ConsoleWriter.WriteInGreen($"Reading dir {photoDir}..");
            var directoryInfo = new DirectoryInfo(photoDir);


            FileInfo sentFile = directoryInfo.GetFiles(sentFileName).FirstOrDefault();
            if (sentFile == null)
            {
                string sentFilePath = Path.Combine(directoryInfo.FullName, sentFileName);
                File.WriteAllText(sentFilePath, "");
                sentFile = directoryInfo.GetFiles(sentFileName).Single();
                if(sentFile == null)
                {
                    throw new IOException($"Could not open sent file at {sentFilePath}");
                }
            }

            var sentIds = new HashSet<string>(File.ReadAllLines(sentFile.FullName).Where(l => !string.IsNullOrWhiteSpace(l)));
            var ignoredIds = new HashSet<FileInfo>();
            var imagesToSend = new HashSet<FileInfo>();

            List<FileInfo> photoFiles = directoryInfo.GetFiles().Where(f => !f.Name.EndsWith(".txt")).ToList();
            foreach (FileInfo photoFile in photoFiles)
            {
                if (sentIds.Contains(photoFile.Name))
                {
                    ignoredIds.Add(photoFile);
                }
                else
                {
                    imagesToSend.Add(photoFile);
                }
            }

            if (imagesToSend.Count == 0)
            {
                ConsoleWriter.WriteInYellow("All photos were already sent - aborting");
                return;
            }

            ConsoleWriter.WriteInGreen($"About to send {imagesToSend.Count} emails, {ignoredIds.Count} were already sent - proceed? (y/n)");
            string input = Console.ReadLine() ?? "n";
            if (!"y".Equals(input.Trim().ToLowerInvariant()))
            {
                ConsoleWriter.WriteInYellow("Aborting");
                return;
            }

            foreach (FileInfo toSend in imagesToSend)
            {
                try
                {
                    ConsoleWriter.WriteInGreen($"Sending {toSend.Name}..");
                    _messageProvider.SendMessage("TAXES AUTO 2019", toSend.Name, toSend.FullName);
                }
                catch (Exception ex)
                {
                    ConsoleWriter.WriteInRed($"Failed to send message, aborting: {ex.Message}");
                    return;
                }

                try
                {
                    File.AppendAllLines(sentFile.FullName, new[] {toSend.Name});
                }
                catch (Exception ex)
                {
                    ConsoleWriter.WriteInRed($"Could not update sent file with {toSend.Name} - aborting: {ex.Message}");
                    return;
                }
            }

            ConsoleWriter.WriteInGreen("Upload completed");
        }

        private static void PrintMessages(ConsoleCommand command)
        {
            string labelId = command.Current;

            ConsoleWriter.WriteInGreen($"Fetching messages with label {labelId}..");

            var messages = _messageProvider.GetMessagesWithLabel(TraceId.New(), labelId);
            if (!messages.Any())
            {
                ConsoleWriter.WriteInGreen($"No messages found for label {labelId}");
                return;
            }

            foreach (var message in messages.OrderBy(m => m.Id))
            {
                ConsoleWriter.WriteInGreen($"{message.Id}\t{message.Date}\t{message.Subject}");
            }
        }

        private static void ReadMessage(ConsoleCommand command)
        {
            string messageId = command.Current;

            ConsoleWriter.WriteInGreen($"Reading message {messageId}..");

            Message message = _messageProvider.GetMessage(TraceId.New(), messageId);
            ConsoleWriter.WriteInGreen(message.Body);
        }

        private static void DownloadWithMessageId(ConsoleCommand command)
        {
            string messageId = command.Current;
            string dir = command.Advance().Current;

            Directory.CreateDirectory(dir);

            ConsoleWriter.WriteInGreen($"Downloading message {messageId} to {dir}..");
            Message message = _messageProvider.GetMessage(TraceId.New(), messageId);
            SaveMessage(message, dir);
        }

        private static void DownloadWithLabel(ConsoleCommand command)
        {
            string labelId = command.Current;
            string dir = command.Advance().Current;
            Directory.CreateDirectory(dir);

            ConsoleWriter.WriteInGreen($"Downloading messages with label {labelId} to {dir}..");
            var messages = _messageProvider.GetMessagesWithLabel(TraceId.New(), labelId);
            if (!messages.Any())
            {
                ConsoleWriter.WriteInGreen($"No messages found for label {labelId}");
                return;
            }

            foreach (var message in messages)
            {
                SaveMessage(message, dir);
            }
        }

        private static void SaveMessage(Message message, string dir)
        {
            ConsoleWriter.WriteInGreen($"Saving {message.Id}\t{message.Date}\t{message.Subject}..");
            string msgDir = Path.Combine(dir, message.Id);
            Directory.CreateDirectory(msgDir);

            File.WriteAllText(Path.Combine(msgDir, "body.txt"), string.Join(Environment.NewLine, message.Date, message.Subject, message.Body));
            foreach (var attachment in message.Attachments)
            {
                File.WriteAllBytes(Path.Combine(msgDir, attachment.Name), attachment.Data);
            }
        }

        private static void PrintLabels()
        {
            ConsoleWriter.WriteInGreen("Fetching labels..");

            List<Label> labels = _labelProvider.GetLabels();
            if (!labels.Any())
            {
                ConsoleWriter.WriteInGreen("No labels found");
                return;
            }
            
            foreach (var label in labels)
            {
                ConsoleWriter.WriteInGreen($"{label.LabelId}\t{label.LabelName}");
            }
        }
    }
}
