using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BAL
{
    public class Doorphone : IDisposable
    {
        #region Events webpage constants

        private const string HttpGetCommand = " GET /events.txt HTTP/1.1";
        private const string BoldClientDefaultUserAgentHttpHeader = "User-Agent: UDVPanel_3.1";
        private const string BoldClientHostHttpHeader = "Host: ";

        #endregion

        private readonly Socket _socket;

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

        private readonly IPAddress _boldAddress;
        private readonly int _boldPort;

        public Doorphone(IPAddress address, int port = 80)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IsInitialized = false;
            _boldAddress = address;
            _boldPort = port;
        }

        private List<BoldBaseEvent> _lastBoldEvents = new List<BoldBaseEvent>();

        public async Task<bool> InitializeAsync()
        {
            AsyncAutoResetEvent autoResetEvent = new AsyncAutoResetEvent();
            SocketAsyncEventArgs args = new SocketAsyncEventArgs
            {
                RemoteEndPoint = new IPEndPoint(_boldAddress, _boldPort)
            };
            args.Completed += SocketConnect_Completed;
            args.UserToken = autoResetEvent;
            bool isPending = _socket.ConnectAsync(args);
            if (isPending)
            {
                await autoResetEvent.WaitOne();
            }
            if (args.LastOperation != SocketAsyncOperation.Connect)
            {
                return false;
            }
            string data = await GetEventListAsync(_socket, _boldAddress);
            List<BoldBaseEvent> boldEvents = new List<BoldBaseEvent>();
            using (BoldEventsStreamReader eventReader = new BoldEventsStreamReader(data))
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
            return IsInitialized = true;
        }

        private static async Task<string> GetEventListAsync(Socket socket, IPAddress ip)
        {
            AsyncAutoResetEvent autoResetEvent = new AsyncAutoResetEvent();
            byte[] data = CreateEventListRequest(ip);
            byte[] buffer = CreateEventListRequest(ip);
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(buffer, 0, buffer.Length);
            args.UserToken = autoResetEvent;
            args.Completed += SocketConnect_Completed;
            bool isPending = socket.SendAsync(args);
            if (isPending)
            {
                await autoResetEvent.WaitOne();
            }
            buffer = new byte[65536];
            args.SetBuffer(buffer, 0, buffer.Length);
            isPending = socket.ReceiveAsync(args);
            if (isPending)
            {
                await autoResetEvent.WaitOne();
            }

            return Encoding.ASCII.GetString(args.Buffer, 0, args.BytesTransferred);
        }

        private static byte[] CreateEventListRequest(IPAddress ip)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(HttpGetCommand);
            sb.AppendLine(BoldClientDefaultUserAgentHttpHeader);
            sb.AppendLine(string.Concat(BoldClientHostHttpHeader, ip));
            sb.AppendLine();
            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        private static void SocketConnect_Completed(object sender, SocketAsyncEventArgs e)
        {
            AsyncAutoResetEvent autoResetEvent = (AsyncAutoResetEvent) e.UserToken;
            autoResetEvent.Set();
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
            string data = await GetEventListAsync(_socket, _boldAddress);
            using (BoldEventsStreamReader eventReader = new BoldEventsStreamReader(data))
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

        internal void OnBoldEvent(BoldBaseEvent oldBoldEvent, BoldBaseEvent newBoldEvent)
        {
            if (newBoldEvent.HasEventValueChange(oldBoldEvent))
            {
                BoldEventHandlerArgs args = newBoldEvent.CreateBoldEventHandlerArgs();
                BoldEvent?.Invoke(this, args);
            }
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }

    public delegate void BoldEventHandler(object sender, BoldEventHandlerArgs args);

    public class BoldEventHandlerArgs
    {
    }
}