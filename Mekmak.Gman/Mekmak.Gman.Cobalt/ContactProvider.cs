using Google.Apis.PeopleService.v1;
using Mekmak.Gman.Silk.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using Google.Apis.PeopleService.v1.Data;
using System.Threading.Tasks;

namespace Mekmak.Gman.Cobalt
{
    public class ContactProvider
    {
        private readonly ITraceLogger _log;
        private readonly PeopleServiceService _peopleService;
        private bool _isCacheWarm = false;
        private readonly object _cacheWarmUpLock = new object();

        public ContactProvider(ITraceLogger log, PeopleServiceService peopleService)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _peopleService = peopleService ?? throw new ArgumentNullException(nameof(peopleService));
        }

        private void EnsureHotCache(string traceId)
        {
            if (_isCacheWarm)
            {
                return;
            }

            lock(_cacheWarmUpLock)
            {
                if(_isCacheWarm)
                {
                    return;
                }

                async void SendEmptyQuery()
                {
                    var request = _peopleService.People.SearchContacts();
                    request.Query = "";
                    request.ReadMask = "metadata,names";
                    await request.ExecuteAsync();
                }

                _log.Info(traceId, "EnsureHotCache.WarmingUp");
                SendEmptyQuery();
                _isCacheWarm = true;
            }
        }

        public async Task<List<Person>> SearchByEmail(string traceId, string email)
        {
            _log.Info(traceId, "SearchByEmail.Querying", $"email={email}");

            EnsureHotCache(traceId);

            var request = _peopleService.People.SearchContacts();
            request.Query = email;
            request.ReadMask = "metadata,names";
            var response = await request.ExecuteAsync();
            return response.Results.Select(r => r.Person).ToList();
        }
    }

}
