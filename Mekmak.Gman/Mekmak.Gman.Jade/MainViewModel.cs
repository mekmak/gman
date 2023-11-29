using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mekmak.Gman.Jade.Models;
using Mekmak.Gman.Ore;
using Exception = System.Exception;

namespace Mekmak.Gman.Jade
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Emails = new ObservableCollection<EmailModel>();
            LoadCommand = new Command(Load);
            RotateImageCommand = new Command(RotateImage);
            ExportCommand = new Command(Export);
            NextEmailCommand = new Command(NextEmail);
        }

        #region Commands

        private Command _nextEmailCommand;
        public Command NextEmailCommand
        {
            get => _nextEmailCommand;
            set
            {
                if (_nextEmailCommand != value)
                {
                    _nextEmailCommand = value;
                    OnPropertyChanged(nameof(NextEmailCommand));
                }
            }
        }

        private Command _rotateImageCommand;
        public Command RotateImageCommand
        {
            get => _rotateImageCommand;
            set
            {
                if (_rotateImageCommand != value)
                {
                    _rotateImageCommand = value;
                    OnPropertyChanged(nameof(RotateImageCommand));
                }
            }
        }

        private Command _loadCommand;
        public Command LoadCommand
        {
            get => _loadCommand;
            set
            {
                if (_loadCommand != value)
                {
                    _loadCommand = value;
                    OnPropertyChanged(nameof(LoadCommand));
                }
            }
        }

        private Command _exportCommand;
        public Command ExportCommand
        {
            get => _exportCommand;
            set
            {
                if (_exportCommand != value)
                {
                    _exportCommand = value;
                    OnPropertyChanged(nameof(ExportCommand));
                }
            }
        }

        #endregion

        #region Properties

        private string _emailDirectory;
        public string EmailDirectory
        {
            get => _emailDirectory;
            set
            {
                if (_emailDirectory != value)
                {
                    _emailDirectory = value;
                    OnPropertyChanged(nameof(EmailDirectory));
                }
            }
        }

        private ObservableCollection<EmailModel> _emails;
        public ObservableCollection<EmailModel> Emails
        {
            get => _emails;
            set
            {
                if (_emails != value)
                {
                    _emails = value;
                    OnPropertyChanged(nameof(Emails));
                }
            }
        }

        private EmailModel _selectedEmail;
        public EmailModel SelectedEmail
        {
            get => _selectedEmail;
            set
            {
                if (_selectedEmail != value)
                {
                    SaveEmailData(_selectedEmail);
                    _selectedEmail = value;
                    OnPropertyChanged(nameof(SelectedEmail));
                }
            }
        }

        private string _userMessage;
        public string UserMessage
        {
            get => _userMessage;
            set
            {
                if (_userMessage != value)
                {
                    _userMessage = value;
                    OnPropertyChanged(nameof(UserMessage));
                }
            }
        }

        #endregion

        private const string DataFileName = "data.json";
        private void SaveEmailData(EmailModel model)
        {
            if(model == null)
            {
                return;
            }

            try
            {
                var dataFilePath = Path.Combine(model.DirectoryPath, DataFileName);
                var data = model.ToEmail().Serialize();
                File.WriteAllText(dataFilePath, data);
                ReportToUser($"Saved email '{model.Subject}' ({model.Id})");
            }
            catch (Exception ex)
            {
                ReportToUser($"Could not save email '{model.Id}': {ex.Message}");
            }
            
        }

        private void NextEmail()
        {
            if (_emails == null || _emails.Count == 0)
            {
                return;
            }

            if(SelectedEmail == null)
            {
                SelectedEmail = _emails.First();
                return;
            }

            int index = _emails.IndexOf(SelectedEmail);
            if (index < 0)
            {
                return;
            }

            if (index == _emails.Count - 1)
            {
                SelectedEmail = _emails.First();
                return;
            }

            SelectedEmail = _emails[index + 1];
        }

        private const string ExportDir = "Exports";
        private void Export()
        {
            if (_emails == null || _emailDirectory == null)
            {
                ReportToUser("Could not export - no emails loaded");
                return;
            }

            var taggedEmails = _emails.Where(e => e.IsTagged).ToList();
            if (taggedEmails.Count == 0)
            {
                ReportToUser("Could not export - no tagged emails found");
                return;
            }

            try
            {
                var csvLines = taggedEmails.Select(e => $"{e.Category},\"{e.AmountDisplay}\",{e.EmailDisplayDate},{e.ReceiptDisplayDate},{e.Source},{e.Gig}").ToList();
                csvLines.Insert(0, "Category,Amount,EmailDate,ReceiptDate,Source,Gig");
                var exportDir = Path.Combine(_emailDirectory, ExportDir);
                Directory.CreateDirectory(exportDir);
                var tempFileName = Path.Combine(exportDir, $"{DateTime.Now:yyyy_MMMM_dd}_{Path.GetRandomFileName()}.txt");
                File.WriteAllLines(tempFileName, csvLines);
                ReportToUser($"Exported to: {tempFileName}");
            }
            catch(Exception ex)
            {
                ReportToUser($"Error exporting: {ex.Message}");
            }
        }

        private void Load()
        {
            if (string.IsNullOrWhiteSpace(_emailDirectory))
            {
                ReportToUser("Could not load - please set directory path");
                return;
            }

            if (!Directory.Exists(_emailDirectory))
            {
                ReportToUser("Could not load - cannot find given directory");
                return;
            }


            int dataReadProblemCount = 0;
            int dataReadSuccessCount = 0;

            var emails = new List<EmailModel>();
            var directoryInfo = new DirectoryInfo(_emailDirectory);
            DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();

            int rowNum = 1;
            foreach (DirectoryInfo subDirectory in subDirectories)
            {
                // Skipping the Exports directory
                if (ExportDir.Equals(subDirectory.Name))
                {
                    continue;
                }

                var emailModel = new EmailModel
                {
                    Id = subDirectory.Name,
                    RowNum = rowNum++,
                    DirectoryPath = subDirectory.FullName
                };

                FileInfo bodyFile = subDirectory.GetFiles("*.txt").FirstOrDefault();
                if (bodyFile != null)
                {
                    try
                    {
                        string body = File.ReadAllText(bodyFile.FullName);
                        emailModel.Body = body;
                        emailModel.Subject = GetSubject(body) ?? "Unknown";
                        emailModel.EmailDate = GetDate(body);
                    }
                    catch (Exception e)
                    {
                        emailModel.Body = $"Error reading file: {e.Message}";
                    }
                }

                try
                {
                    Infer(emailModel);
                }
                catch
                {
                    // ignored
                }

                FileInfo imageFile = subDirectory.GetFiles().FirstOrDefault(f => !f.Name.EndsWith(".txt") && !f.Name.EndsWith(".json"));
                if (imageFile != null)
                {
                    emailModel.Uri = imageFile.FullName;
                }

                FileInfo dataFile = subDirectory.GetFiles(DataFileName).FirstOrDefault();
                if (dataFile != null)
                {
                    try
                    {
                        string data = File.ReadAllText(dataFile.FullName);
                        Email email = Email.Deserialize(data);
                        emailModel.OverwriteWith(email);
                        dataReadSuccessCount++;
                    }
                    catch
                    {
                        // ignored
                        dataReadProblemCount++;
                    }
                }

                emails.Add(emailModel);
            }

            Emails = new ObservableCollection<EmailModel>(emails);
            SelectedEmail = Emails.FirstOrDefault();
            ReportToUser($"Loaded {Emails.Count} email(s) - {dataReadSuccessCount} data file(s) processed, {dataReadProblemCount} had issues");
        }

        private static readonly Regex MtaRegex = new Regex(@"^Your \$(?<price>.*?) transaction with MTA\*NYCT PAYGO$", RegexOptions.Compiled);
        private void Infer(EmailModel email)
        {
            if (MtaRegex.IsMatch(email.Subject))
            {
                var matches = MtaRegex.Matches(email.Subject);
                var price = decimal.Parse(matches[0].Groups["price"].Value);
                email.Amount = price;
                email.Category = "Travel";
                email.Gig = "Musician";
                email.ReceiptDate = email.EmailDate;
            }
        }

        private string GetSubject(string emailBody)
        {
            string[] lines = emailBody.Split(Environment.NewLine);
            return lines.Length < 2 ? null : lines[1].Trim().Substring(0, Math.Min(lines[1].Length, 250));
        }

        private DateTime? GetDate(string emailBody)
        {
            string[] lines = emailBody.Split(Environment.NewLine);
            if (lines.Length == 0)
            {
                return null;
            }

            string dateLine = lines[0];

            {
                if (DateTime.TryParse(dateLine, out DateTime date))
                {
                    return date;
                }
            }

            {
                string trimmedDateLine = dateLine.Substring(0, dateLine.Length - 6);
                if (DateTime.TryParseExact(trimmedDateLine, "ddd, d MMM yyyy H:mm:ss K", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime date))
                {
                    return date;
                }
            }

            return null;
        }

        private void RotateImage()
        {
            if (SelectedEmail == null)
            {
                return;
            }

            SelectedEmail.ImageRotateAngle += 90;
            if (SelectedEmail.ImageRotateAngle >= 360)
            {
                SelectedEmail.ImageRotateAngle = 0;
            }
        }

        private void ReportToUser(string message)
        {
            UserMessage = $"[{DateTime.Now:HH:mm:ss}]: {message}";
        }
    }
}
