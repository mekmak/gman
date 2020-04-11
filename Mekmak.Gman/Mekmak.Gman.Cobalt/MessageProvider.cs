using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Mekmak.Gman.Ore;
using Mekmak.Gman.Silk.Interfaces;
using Message = Mekmak.Gman.Ore.Message;

namespace Mekmak.Gman.Cobalt
{
    public class MessageProvider
    {
        private static readonly ConcurrentDictionary<string, Message> MessageCache = new ConcurrentDictionary<string, Message>();

        private readonly ITraceLogger _log;
        private readonly GmailService _service;

        public MessageProvider(ITraceLogger log, GmailService service)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public List<Message> GetMessagesWithLabel(string traceId, params string[] labelIds)
        {
            var request = _service.Users.Messages.List("me");
            request.LabelIds = labelIds;

            var messageIds = request.Execute().Messages.Select(m => m.Id).ToList();
            var messages = new ConcurrentBag<Message>();

            Parallel.ForEach(messageIds, id =>
            {
                Message message = GetMessage(traceId, id);
                messages.Add(message);
            });

            return messages.ToList();
        }

        public Message GetMessage(string traceId, string messageId)
        {
            if (MessageCache.TryGetValue(messageId, out var msg))
            {
                return msg;
            }

            var request = _service.Users.Messages.Get("me", messageId);
            request.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;

            var gMessage = request.Execute();
            
            List<Header> headers = GetHeaders(gMessage.Payload).ToList();
            List<string> bodyParts = GetBody(gMessage.Payload).ToList();

            var message = new Message
            {
                Id = gMessage.Id,
                Body = string.Join(Environment.NewLine, bodyParts),
                Headers = headers
            };

            List<Tuple<string, string>> attachmentIds = GetAttachmentIds(gMessage.Payload).ToList();
            foreach (Tuple<string,string> attachmentId in attachmentIds)
            {
                try
                {
                    byte[] data = GetAttachment(messageId, attachmentId.Item1);
                    message.Attachments.Add(new Attachment
                    {
                        Id = attachmentId.Item1,
                        Name = attachmentId.Item2,
                        MessageId = messageId,
                        Data = data
                    });
                }
                catch (Exception ex)
                {
                    _log.Info(traceId, "GetMessage.Attachment.Exception", $"id={attachmentId.Item1}, name={attachmentId.Item2}, msgId={message.Id}, exception=\"{ex.Message}\"");
                }
            }

            MessageCache.AddOrUpdate(messageId, message, (id, m) => message);
            return message;
        }

        private byte[] GetAttachment(string messageId, string attachmentId)
        {
            var request = _service.Users.Messages.Attachments.Get("me", messageId, attachmentId);
            MessagePartBody part = request.Execute();
            return DecompressToBytes(part.Data);
        }

        private IEnumerable<Header> GetHeaders(MessagePart part)
        {
            if (part.Headers != null)
            {
                foreach (var header in part.Headers)
                {
                    yield return new Header { PartId = part.PartId, Name = header.Name, Value = header.Value };
                }
            }

            if (part.Parts != null)
            {
                foreach (var header in part.Parts.SelectMany(GetHeaders))
                {
                    yield return header;
                }
            }
        }

        private IEnumerable<Tuple<string,string>> GetAttachmentIds(MessagePart part)
        {
            if (!string.IsNullOrWhiteSpace(part.Filename))
            {
                yield return Tuple.Create(part.Body.AttachmentId, part.Filename);
            }

            if (part.Parts != null)
            {
                foreach (var id in part.Parts.SelectMany(GetAttachmentIds))
                {
                    yield return id;
                }
            }
        }

        private IEnumerable<string> GetBody(MessagePart part)
        {
            if (part.Body?.Data != null)
            {
                string data = DecompressToString(part.Body.Data);
                yield return data;
            }

            if (part.Parts == null)
            {
                yield break;
            }

            foreach (string data in part.Parts.SelectMany(GetBody))
            {
                yield return data;
            }
        }

        private byte[] DecompressToBytes(string base64String)
        {
            return string.IsNullOrWhiteSpace(base64String) ? new byte[0] : Convert.FromBase64String(SanitizeBase64String(base64String));
        }

        private string DecompressToString(string base64String)
        {
            if (string.IsNullOrWhiteSpace(base64String))
            {
                return base64String;
            }

            try
            {
                var base64EncodedBytes = Convert.FromBase64String(SanitizeBase64String(base64String));
                return Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception ex)
            {
                return $"Error decompressing: {ex.Message}" + Environment.NewLine + base64String;
            }
        }

        private string SanitizeBase64String(string str)
        {
            return str.Replace("-", "+").Replace("_", "/");
        }
    }
}
