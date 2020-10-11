using System;
using Mekmak.Gman.Ore;

namespace Mekmak.Gman.Jade.Models
{
    public class EmailModel : ObservableModel
    {
        public int RowNum { get; set; }

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

        private DateTime? _emailDate;

        public DateTime? EmailDate
        {
            get => _emailDate;
            set
            {
                if (_emailDate != value)
                {
                    _emailDate = value;
                    OnPropertyChanged(nameof(EmailDate));
                    OnPropertyChanged(nameof(EmailDisplayDate));
                }
            }
        }

        public string EmailDisplayDate => EmailDate == null ? "" : $"{EmailDate:d}";

        private DateTime? _receiptDate;
        public DateTime? ReceiptDate
        {
            get => _receiptDate;
            set
            {
                if (_receiptDate != value)
                {
                    _receiptDate = value;
                    UpdateReceiptDateParts();
                    OnPropertyChanged(nameof(ReceiptDate));
                    OnPropertyChanged(nameof(ReceiptDisplayDate));
                    OnPropertyChanged(nameof(ReceiptDateDay));
                    OnPropertyChanged(nameof(ReceiptDateMonth));
                    OnPropertyChanged(nameof(ReceiptDateYear));
                }
            }
        }

        private int? _receiptDateDay;
        public int? ReceiptDateDay
        {
            get => _receiptDateDay;
            set
            {
                if(_receiptDateDay != value)
                {
                    _receiptDateDay = value;
                    UpdateReceiptDate();
                }
            }
        }

        private int? _receiptDateMonth;
        public int? ReceiptDateMonth
        {
            get => _receiptDateMonth;
            set
            {
                if (_receiptDateMonth != value)
                {
                    _receiptDateMonth = value;
                    UpdateReceiptDate();
                }
            }
        }

        private int? _receiptDateYear;
        public int? ReceiptDateYear
        {
            get => _receiptDateYear;
            set
            {
                if(_receiptDateYear != value)
                {
                    _receiptDateYear = value;
                    UpdateReceiptDate();
                }
            }
        }

        private void UpdateReceiptDateParts()
        {
            if (_receiptDate.HasValue)
            {
                _receiptDateDay = _receiptDate.Value.Day;
                _receiptDateMonth = _receiptDate.Value.Month;
                _receiptDateYear = _receiptDate.Value.Year;
            }
            else
            {
                _receiptDateDay = null;
                _receiptDateMonth = null;
                _receiptDateYear = null;
            }
        }

        private void UpdateReceiptDate()
        {
            if (!(_receiptDateYear.HasValue && _receiptDateMonth.HasValue && _receiptDateDay.HasValue))
            {
                return;
            }

            ReceiptDate = new DateTime(_receiptDateYear.Value, _receiptDateMonth.Value, _receiptDateDay.Value);
        }

        public string ReceiptDisplayDate => ReceiptDate == null ? "" : $"{ReceiptDate:d}";

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

        public string Source => $"Email ({Id})";

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

        private decimal? _amount;
        public decimal? Amount
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

        public string AmountDisplay => Amount.HasValue ? $"{Amount:C}" : $"{0:C}";

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
            ReceiptDate = email.ReceiptDate;
            EmailDate = email.EmailDate ?? EmailDate;
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
                ReceiptDate = ReceiptDate,
                EmailDate = EmailDate,
                Gig = Gig,
                Subject = Subject,
                ImageRotateAngle = ImageRotateAngle
            };
        }
    }
}
