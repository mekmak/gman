using System;
using Mekmak.Gman.Ore;

namespace Mekmak.Gman.Jade.Models
{
    public class EmailModel : ObservableModel
    {
        private string _id;
        public string Id
        {
            get => _id;
            set
            {
                if(_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string DirectoryPath { get; set; }

        private string _subject;
        public string Subject
        {
            get => _subject;
            set
            {
                if(_subject != value)
                {
                    _subject = value;
                    OnPropertyChanged(nameof(Subject));
                }
            }
        }

        private DateTime? _date;
        public DateTime? Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged(nameof(Date));
                    OnPropertyChanged(nameof(DisplayDate));
                }
            }
        }

        public string DisplayDate => Date == null ? "" : $"{Date:d}";

        private string _body;
        public string Body
        {
            get => _body;
            set
            {
                if(_body != value)
                {
                    _body = value;
                    OnPropertyChanged(nameof(Body));
                }
            }
        }

        private string _uri;
        public string Uri
        {
            get => _uri;
            set
            {
                if(_uri != value)
                {
                    _uri = value;
                    OnPropertyChanged(nameof(Uri));
                }
            }
        }

        private double _imageRotateAngle;
        public double ImageRotateAngle
        {
            get => _imageRotateAngle;
            set
            {
                if(Math.Abs(_imageRotateAngle - value) > double.Epsilon)
                {
                    _imageRotateAngle = value;
                    OnPropertyChanged(nameof(ImageRotateAngle));
                }
            }
        }

        public string Source => "Email";

        public bool IsTagged
        {
            get => _amount > 0 && !string.IsNullOrWhiteSpace(Gig) && !string.IsNullOrWhiteSpace(Category);
            set { }
        }

        private string _category;
        public string Category
        {
            get => _category;
            set
            {
                if (_category != value)
                {
                    _category = value;

                    OnPropertyChanged(nameof(Category));
                    OnPropertyChanged(nameof(IsTagged));
                }
            }
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set
            {
                if(_amount != value)
                {
                    _amount = value;

                    OnPropertyChanged(nameof(Amount));
                    OnPropertyChanged(nameof(AmountDisplay));
                    OnPropertyChanged(nameof(IsTagged));
                }
            }
        }

        public string AmountDisplay => $"{Amount:C}";

        private string _gig;
        public string Gig
        {
            get => _gig;
            set
            {
                if (_gig != value)
                {
                    _gig = value;

                    OnPropertyChanged(nameof(Gig));
                    OnPropertyChanged(nameof(IsTagged));
                }
            }
        }

        public void OverwriteWith(Email email)
        {
            Id = email.Id;
            Amount = email.Amount;
            Body = email.Body;
            Category = email.Category;
            Date = email.Date;
            Gig = email.Gig;
            Subject = email.Subject;
            ImageRotateAngle = email.ImageRotateAngle;
        }

        public Email ToEmail()
        {
            return new Email
            {
                Id = Id,
                Amount = Amount,
                Body = Body,
                Category = Category,
                Date = Date,
                Gig = Gig,
                Subject = Subject,
                ImageRotateAngle = ImageRotateAngle
            };
        }
    }
}
