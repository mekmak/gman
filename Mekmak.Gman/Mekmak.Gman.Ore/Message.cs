using System.Collections.Generic;
using System.Linq;

namespace Mekmak.Gman.Ore
{
    public class Message
    {
        public Message()
        {
            Attachments = new List<Attachment>();
            Headers = new List<Header>();
        }

        public string Id { get; set; }
        public string Subject => GetHeader("Subject");
        public string From => GetHeader("From");
        public string Date => GetHeader("Date");
        public string Body { get; set; }

        private string GetHeader(string headerName)
        {
            var headers = Headers.Where(h => h.Name == headerName).ToList();
            return !headers.Any() ? "[Missing]" : string.Join(", ", headers.Select(h => h.Value));
        }

        public List<Header> Headers { get; set; }

        public List<Attachment> Attachments { get; set; }
    }
}
