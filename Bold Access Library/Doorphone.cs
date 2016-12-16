using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.BAL
{
    public class Doorphone : IDisposable
    {
        #region Events webpage constants

        private const string HttpGetCommand = "GET /events.txt HTTP/1.1";
        private const string BoldClientDefaultUserAgentHttpHeader = "User-Agent: UDVPanel_3.1";
        private const string BoldClientHostHttpHeader = "Host: ";

        #endregion

        #region Public properties

        public string Hostname { get; private set; }
        private const string HostnameKey = "NET_HOSTNAME";
        public IPAddress Ip { get; private set; }
        private const string IpKey = "IP_ADDR";
        public string FwVersion { get; private set; }
        private const string FwVersionKey = "VERSION";

        public bool IsInitialized { get; private set; }
        public event BoldEventHandler BoldEvent;

        public event BoldImageEventHandler BoldImageEventHandler;

        #endregion

        #region Private properties

        private readonly IPEndPoint _remoteEndPoint;
        private readonly BackgroundWorker _backgroundWorker;
        private readonly Socket _socket;

        #endregion

        #region Constructor

        public Doorphone(IPAddress address, int port = 80)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IsInitialized = false;

            _remoteEndPoint = new IPEndPoint(address, port);
            _backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
        }

        #endregion

        #region Public methods

        public async Task<bool> InitializeAsync()
        {
            SocketAsyncEventArgs args = await SocketHelper.ConnectAsync(_socket, _remoteEndPoint);

            if (args.LastOperation != SocketAsyncOperation.Connect)
            {
                return false;
            }

            string data = await GetEventListAsync(_socket, _remoteEndPoint.Address);
            ReadBoldSettings(data);
            _backgroundWorker.DoWork += BackgroundWorker_DoWork;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            _backgroundWorker.RunWorkerAsync();
            return IsInitialized = true;
        }

        public void Dispose()
        {
            if (IsInitialized)
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            _socket.Dispose();
        }

        #endregion

        #region Private & Internal methods

        private void ReadBoldSettings(string data)
        {
            using (BoldEventsStreamReader eventReader = new BoldEventsStreamReader(data))
            {
                Dictionary<string, string> configValues = eventReader.GetSetting();
                Hostname = configValues.Where(val => val.Key.Equals(HostnameKey)).Select(val => val.Value).First();
                string ipString = configValues.Where(val => val.Key.Equals(IpKey)).Select(val => val.Value).First();
                Ip = new IPAddress(ipString.Split('.').Select(byte.Parse).ToArray());
                FwVersion = configValues.Where(val => val.Key.Equals(FwVersionKey)).Select(val => val.Value).First();
            }
        }

        private IEnumerable<BoldBaseEvent> GetBoldEvents(string data)
        {
            List<BoldBaseEvent> boldEvents = new List<BoldBaseEvent>();
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
            return boldEvents;
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Task<string> task = GetEventListAsync(_socket, _remoteEndPoint.Address);
            Task.WaitAll(task);
            IEnumerable<BoldBaseEvent> boldEvents = GetBoldEvents(task.Result);
            foreach (BoldBaseEvent boldEvent in boldEvents)
            {
                OnBoldEvent(boldEvent);
            }
        }

        private static async Task<string> GetEventListAsync(Socket socket, IPAddress ip)
        {
            byte[] buffer = CreateEventListRequest(ip);
            await SocketHelper.SendAsync(socket, buffer);
            SocketAsyncEventArgs saea = await SocketHelper.ReceiveAsync(socket);
            return Encoding.ASCII.GetString(saea.Buffer, 0, saea.BytesTransferred);
        }

        private static byte[] CreateEventListRequest(IPAddress ip)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(HttpGetCommand);
            sb.AppendLine(BoldClientDefaultUserAgentHttpHeader);
            sb.AppendLine(string.Concat(BoldClientHostHttpHeader, ip));
            sb.AppendLine();
            string request = sb.ToString();
            return Encoding.ASCII.GetBytes(request);
        }


        internal void OnBoldEvent(BoldBaseEvent boldEvent)
        {
            BoldEventHandlerArgs args = boldEvent.CreateBoldEventHandlerArgs();
            BoldEvent?.Invoke(this, args);
        }

        #endregion

        protected virtual void OnBoldImageEventHandler(BoldImageEventHandlerArgs args)
        {
            BoldImageEventHandler?.Invoke(this, args);
        }
    }

    public delegate void BoldEventHandler(object sender, BoldEventHandlerArgs args);

    public delegate void BoldImageEventHandler(object sender, BoldImageEventHandlerArgs args);

    public class BoldEventHandlerArgs
    {
    }

    public class BoldImageEventHandlerArgs : BoldEventHandlerArgs
    {
        //public BoldImageEventHandlerArgs(ImageStream imageStream)
        //{
        //    ImageStream = imageStream;
        //}

        //Windows.Graphics.Imaging.SoftwareBitmap
        //    Windows.Graphics.Imaging.ImageStream ImageStream;
    }
}