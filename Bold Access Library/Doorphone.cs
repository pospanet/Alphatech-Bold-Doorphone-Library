using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BAL
{
    public class Doorphone : IDisposable
    {
        #region Events webpage constants

        private const string UrlPattern = "http://{0}:{1}";
        private const int HttpTimeout = 30;
        private const string BoldClientDefaultUserAgent = "UDVPanel_3.1";
        private const string EventsPath = "events.txt";

        #endregion

        private readonly HttpClient _httpClient;

        #region Properties

        public string Hostname { get; private set; }
        private const string HostnameKey = "NET_HOSTNAME";
        public IPAddress Ip { get; private set; }
        private const string IpKey = "IP_ADDR";
        public string FwVersion { get; private set; }
        private const string FwVersionKey = "VERSION";

        #endregion

        public event BoldEventHandler BoldEvent;

        public bool IsInitialized { get; private set; }

        public Doorphone(IPAddress address, int port = 80)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(string.Format(UrlPattern, address, port)),
                Timeout = TimeSpan.FromSeconds(HttpTimeout)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", BoldClientDefaultUserAgent);
            IsInitialized = false;
        }

        private List<BoldBaseEvent> _lastBoldEvents = new List<BoldBaseEvent>();

        public async Task Initialize()
        {
            List<BoldBaseEvent> boldEvents = new List<BoldBaseEvent>();
            Stream dataStream = await GetEventsDataAsync();
            using (BoldEventsStreamReader eventReader = new BoldEventsStreamReader(dataStream))
            {
                Dictionary<string, string> configValues = eventReader.GetSetting();
                Hostname = configValues.Where(val => val.Key.Equals(HostnameKey)).Select(val => val.Value).First();
                string ipString = configValues.Where(val => val.Key.Equals(IpKey)).Select(val => val.Value).First();
                Ip = new IPAddress(ipString.Split('.').Select(byte.Parse).ToArray());
                FwVersion = configValues.Where(val => val.Key.Equals(FwVersionKey)).Select(val => val.Value).First();
                Dictionary<string, string> values = eventReader.GetNextEvent();
                while (values != null)
                {
                    BoldBaseEvent boldEvent = BoldBaseEvent.CreateEvent(values);
                    boldEvents.Add(boldEvent);
                    values = eventReader.GetNextEvent();
                }
            }
            _lastBoldEvents = boldEvents;
            IsInitialized = true;
        }

        public async Task ListenAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await CheckEventsAsync();
            }
        }

        private async Task CheckEventsAsync()
        {
            List<BoldBaseEvent> boldEvents = new List<BoldBaseEvent>();
            Stream dataStream = await GetEventsDataAsync();
            using (BoldEventsStreamReader eventReader = new BoldEventsStreamReader(dataStream))
            {
                Dictionary<string, string> values = eventReader.GetNextEvent();
                while (values != null)
                {
                    BoldBaseEvent boldEvent = BoldBaseEvent.CreateEvent(values);
                    boldEvents.Add(boldEvent);
                    values = eventReader.GetNextEvent();
                }
            }
            foreach (BoldBaseEvent boldEvent in boldEvents)
            {
                IEnumerable<BoldBaseEvent> lastBoldEvent =
                    _lastBoldEvents.Where(e => e.IsSameSourceInternal(boldEvent)).ToArray();
                if (lastBoldEvent.Any())
                {
                    OnBoldEvent(lastBoldEvent.First(), boldEvent);
                }
            }
        }

        private async Task<Stream> GetEventsDataAsync()
        {
            using (HttpResponseMessage response = await _httpClient.GetAsync(EventsPath))
            {
                Stream responseContent = await response.Content.ReadAsStreamAsync();
                MemoryStream outStream = new MemoryStream();
                await responseContent.CopyToAsync(outStream);
                outStream.Seek(0, SeekOrigin.Begin);
                return outStream;
            }
        }

        internal void OnBoldEvent(BoldBaseEvent oldBoldEvent, BoldBaseEvent newBoldEvent)
        {
            if (newBoldEvent.HasEventValueChange(oldBoldEvent))
            {
                BoldEventHandlerArgs args = newBoldEvent.CreateBoldEventHandlerArgs(oldBoldEvent);
                BoldEvent?.Invoke(this, args);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }

    public delegate void BoldEventHandler(object sender, BoldEventHandlerArgs args);

    public class BoldEventHandlerArgs
    {
    }
}