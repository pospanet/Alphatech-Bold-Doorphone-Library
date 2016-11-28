using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.BAL
{
    public class Doorphone
    {
        private const string UrlPattern = "http://{0}:{1}";
        private const int HttpTimeout = 30;
        private const string BoldClientDefaultUserAgent = "UDVPanel_3.1";
        private const string EventsPath = "events.txt";

        private readonly HttpClient httpClient;

        public string Hostname { get; private set; }
        private const string HostnameKey = "NET_HOSTNAME";
        public IPAddress Ip { get; private set; }
        private const string IpKey = "IP_ADDR";
        public string FwVersion { get; private set; }
        private const string FwVersionKey = "VERSION";

        public event DoorEventHandler DoorEvent;

        public Doorphone(IPAddress address, int port = 80)
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(string.Format(UrlPattern, address, port)),
                Timeout = TimeSpan.FromSeconds(HttpTimeout)
            };
            httpClient.DefaultRequestHeaders.Add("User-Agent", BoldClientDefaultUserAgent);
            Initialize();
        }

        private void Initialize()
        {
            Task<Dictionary<string, string>> readTask = ReadConfigAsync();
            Task.WhenAll(readTask);
            Dictionary<string, string> values = readTask.Result;
            Hostname = values.Where(val => val.Key.Equals(HostnameKey)).Select(val => val.Value).First();
            string ipString = values.Where(val => val.Key.Equals(IpKey)).Select(val => val.Value).First();
            Ip = new IPAddress(ipString.Split('.').Select(byte.Parse).ToArray());
            FwVersion = values.Where(val => val.Key.Equals(FwVersionKey)).Select(val => val.Value).First();
        }

        private async Task<IEnumerable<BoldBaseEvent>> ReadEventsAsync()
        {
            List<BoldBaseEvent> boldEvents = new List<BoldBaseEvent>();
            HttpResponseMessage response = await httpClient.GetAsync(EventsPath);
            using (
                BoldEventsStreamReader eventReader =
                    new BoldEventsStreamReader(await response.Content.ReadAsStreamAsync()))
            {
                while (!eventReader.EndOfStream)
                {
                    Dictionary<string, string> values = eventReader.GetNextEvent();
                    BoldBaseEvent boldEvent = BoldBaseEvent.CreateEvent(values);
                    boldEvents.Add(boldEvent);
                }
            }
            return boldEvents;
        }
        private async Task<Dictionary<string, string>> ReadConfigAsync()
        {
            HttpResponseMessage response = await httpClient.GetAsync(EventsPath);
            using (
                BoldEventsStreamReader reader =
                    new BoldEventsStreamReader(await response.Content.ReadAsStreamAsync()))
            {
                return reader.GetSetting();
            }

        }

        protected virtual void OnDoorEvent(DoorEventHandlerArgs args)
        {
            DoorEvent?.Invoke(this, args);
        }
    }

    internal class BoldEventsStreamReader : IDisposable
    {
        private const string SettingSectionHeader = "[setting]";
        private const string EventSectionHeader = "[evstat]";

        private readonly StreamReader _streamReader;
        private string _lastDataLine;

        public BoldEventsStreamReader(Stream stream)
        {
            _streamReader = new StreamReader(stream);
        }

        public bool EndOfStream => _streamReader.EndOfStream;

        public Dictionary<string, string> GetSetting()
        {
            return GetSection(SettingSectionHeader);
        }

        public Dictionary<string, string> GetNextEvent()
        {
            return GetSection(EventSectionHeader);
        }

        private Dictionary<string, string> GetSection(string sectionHeader)
        {
            Dictionary<string, string> sectionData = new Dictionary<string, string>();
            do
            {
                string[] pair = _lastDataLine.Split('=');
                sectionData.Add(pair[0].Trim(), pair[1].Trim('"').Trim());
                _lastDataLine = _streamReader.ReadLine().Trim();
            } while (!string.IsNullOrEmpty(_lastDataLine));
            do
            {
                _lastDataLine = _streamReader.ReadLine().Trim();
            } while (!_lastDataLine.Equals(sectionHeader));
            return sectionData;
        }

        public void Dispose()
        {
            _streamReader.Dispose();
        }
    }

    internal abstract class BoldBaseEvent
    {
        private const string EventTypeKey = "EVENT";
        private const string RegistrationEventTypeKey = "REGISTRATION";
        private const string CallEventTypeKey = "CALL";
        private const string GuardEventTypeKey = "GUARD";

        protected BoldBaseEvent()
        {
        }

        internal static BoldBaseEvent CreateEvent(Dictionary<string, string> values)
        {
            BoldBaseEvent boldBaseEvent;
            KeyValuePair<string, string> eventPair = values.First();
            switch (eventPair.Key)
            {
                case RegistrationEventTypeKey:
                    boldBaseEvent = new RegistrationEvent();
                    break;
                case CallEventTypeKey:
                    boldBaseEvent = new CallEvent();
                    break;
                case GuardEventTypeKey:
                    boldBaseEvent = new GuardEvent();
                    break;
                default:
                    boldBaseEvent = new UnknownBoldEvent();
                    break;
            }
            boldBaseEvent.Initialize(values);
            return boldBaseEvent;
        }

        protected abstract void Initialize(Dictionary<string, string> values);

    }

    internal class UnknownBoldEvent : BoldBaseEvent
    {

        protected override void Initialize(Dictionary<string, string> values)
        {
            throw new NotImplementedException();
        }
    }

    internal class GuardEvent : BoldBaseEvent
    {
        protected override void Initialize(Dictionary<string, string> values)
        {
            throw new NotImplementedException();
        }
    }

    internal class CallEvent : BoldBaseEvent
    {
        protected override void Initialize(Dictionary<string, string> values)
        {
            throw new NotImplementedException();
        }
    }
    internal class RegistrationEvent : BoldBaseEvent
    {
        protected override void Initialize(Dictionary<string, string> values)
        {
            throw new NotImplementedException();
        }
    }

    public delegate void DoorEventHandler(object sender, DoorEventHandlerArgs args);

    public class DoorEventHandlerArgs
    {
        public int DoorId { get; }
        public DoorStatus DoorStatus { get; }
    }

    public enum DoorStatus
    {
        Open,
        Closed
    }
}