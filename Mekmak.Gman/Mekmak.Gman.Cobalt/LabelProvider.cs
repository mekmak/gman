using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Gmail.v1;
using Mekmak.Gman.Ore;

namespace Mekmak.Gman.Cobalt
{
    public class LabelProvider
    {
        private readonly GmailService _service;

        public LabelProvider(GmailService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public List<Label> GetLabels()
        {
            var request = _service.Users.Labels.List("me");
            IList<Google.Apis.Gmail.v1.Data.Label> labels = request.Execute().Labels;
            return labels.Select(l => new Label {LabelId = l.Id, LabelName = l.Name}).ToList();
        }

    }
}
