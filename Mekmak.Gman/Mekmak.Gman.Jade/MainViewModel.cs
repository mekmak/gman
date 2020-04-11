using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Mekmak.Gman.Jade.Models;

namespace Mekmak.Gman.Jade
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Emails = new ObservableCollection<EmailModel>();
            LoadCommand = new Command(Load);
        }

        #region Commands

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
                    _selectedEmail = value;
                    OnPropertyChanged(nameof(SelectedEmail));
                    ImageUri = _selectedEmail.ImageUri;
                }
            }
        }

        private string _imageUri;
        public string ImageUri
        {
            get => _imageUri;
            set
            {
                if (_imageUri != value)
                {
                    _imageUri = value;
                    OnPropertyChanged(nameof(ImageUri));
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

        private void Load()
        {
            if (string.IsNullOrWhiteSpace(_emailDirectory))
            {
                return;
            }

            if (!Directory.Exists(_emailDirectory))
            {
                ReportToUser("Cannot find directory");
                return;
            }


            var emails = new List<EmailModel>();
            var directoryInfo = new DirectoryInfo(_emailDirectory);
            DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
            foreach (DirectoryInfo subDirectory in subDirectories)
            {
                var email = new EmailModel
                {
                    Id = subDirectory.Name
                };

                FileInfo bodyFile = subDirectory.GetFiles("*.txt").FirstOrDefault();
                if (bodyFile != null)
                {
                    try
                    {
                        string body = File.ReadAllText(bodyFile.FullName);
                        email.Body = body;
                    }
                    catch (Exception e)
                    {
                        email.Body = $"Error reading file: {e.Message}";
                    }
                }

                FileInfo imageFile = subDirectory.GetFiles().FirstOrDefault(f => !f.Name.EndsWith(".txt"));
                if (imageFile != null)
                {
                    email.ImageUri = imageFile.FullName;
                }

                emails.Add(email);
            }

            Emails = new ObservableCollection<EmailModel>(emails);
            SelectedEmail = Emails.FirstOrDefault();
        }

        private void ReportToUser(string message)
        {
            UserMessage = $"[{DateTime.Now:HH:mm:ss}]: {message}";
        }
    }
}
