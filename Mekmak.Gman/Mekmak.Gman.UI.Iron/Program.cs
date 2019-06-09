using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Apis.Gmail.v1;
using Mekmak.Gman.Cobalt;
using Mekmak.Gman.Diamond;
using Mekmak.Gman.Ore;
using Mekmak.Gman.Silk.Interfaces;

namespace Mekmak.Gman.UI.Iron
{
    class Program
    {
        private static GmailService _service;
        private static ITraceLogger _log;

        static void Main(string[] args)
        {
            string credentialsPath = args[0];
            string tokenDir = args[1];

            if (string.IsNullOrWhiteSpace(credentialsPath) || string.IsNullOrWhiteSpace(tokenDir))
            {
                throw new ArgumentException("Must pass in credentials file location and token directory");
            }

            _log = new ConsoleTraceLogger("Gman.UI.Iron");

            var factory = new GmailServiceFactory();
            _service = factory.BuildAsync(new GmailServiceConfig
            {
                ApiScopes = new[] {GmailService.Scope.GmailReadonly},
                AppName = "Mekmak Gman",
                CredentialsFilePath = credentialsPath,
                TokenDirectory = tokenDir
            }).Result;
            
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
                    ProcessCommand(command);
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

        private static void ProcessCommand(ConsoleCommand command)
        {
            switch (command.Current)
            {
                case "label":
                    ProcessLabelCommand(command.Advance());
                    return;
                case "message":
                    ProcessMessageCommand(command.Advance());
                    return;
                default:
                    ConsoleWriter.WriteInYellow($"Unknown command '{command}'");
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
                case "-read":
                    ReadMessage(command.Advance());
                    return;
                default:
                    ConsoleWriter.WriteInYellow($"Unknown message command '{command}'");
                    return;
            }
        }

        private static void PrintMessages(ConsoleCommand command)
        {
            string labelId = command.Current;

            ConsoleWriter.WriteInGreen($"Fetching messages with label {labelId}..");

            var messageProvider = new MessageProvider(_log.GetSubLogger("MessageProvider"), _service);
            var messages = messageProvider.GetMessagesWithLabel(TraceId.New(), labelId);
            if (!messages.Any())
            {
                ConsoleWriter.WriteInGreen($"No messages found for label {labelId}");
                return;
            }

            foreach (var message in messages)
            {
                ConsoleWriter.WriteInGreen($"{message.Id}\t{message.Date}\t{message.Subject}");
            }
        }

        private static void ReadMessage(ConsoleCommand command)
        {
            string messageId = command.Current;

            ConsoleWriter.WriteInGreen($"Reading message {messageId}..");

            var messageProvider = new MessageProvider(_log.GetSubLogger("MessageProvider"),_service);
            Message message = messageProvider.GetMessage(TraceId.New(), messageId);
            ConsoleWriter.WriteInGreen(message.Body);
        }

        private static void DownloadWithLabel(ConsoleCommand command)
        {
            string labelId = command.Current;
            string dir = command.Advance().Current;
            Directory.CreateDirectory(dir);

            ConsoleWriter.WriteInGreen($"Downloading messages with label {labelId} to {dir}..");
            var messageProvider = new MessageProvider(_log.GetSubLogger("MessageProvider"),_service);
            var messages = messageProvider.GetMessagesWithLabel(TraceId.New(), labelId);

            if (!messages.Any())
            {
                ConsoleWriter.WriteInGreen($"No messages found for label {labelId}");
                return;
            }

            foreach (var message in messages)
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
        }

        private static void PrintLabels()
        {
            ConsoleWriter.WriteInGreen("Fetching labels..");

            var labelProvider = new LabelProvider(_service);
            List<Label> labels = labelProvider.GetLabels();
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
