using System;
using Newtonsoft.Json;

namespace Mekmak.Gman.Ore
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Email
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("date")]
        public DateTime? Date { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("gig")]
        public string Gig { get; set; }

        [JsonProperty("imageRotateAngle")]
        public double ImageRotateAngle { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Email Deserialize(string serializedEmail)
        {
            return JsonConvert.DeserializeObject<Email>(serializedEmail);
        }
    }
}
