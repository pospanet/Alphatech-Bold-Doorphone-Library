using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Microsoft.Azure.Devices.Client;
using Microsoft.BAL;
using Newtonsoft.Json;

namespace Bold_Extender
{
    public sealed class StartupTask : IBackgroundTask
    {
        private readonly DeviceClient _deviceClient;

        private const string Hostname = "<IoT hub hostname>";
        private static string DeviceId => DeviceInfo.Id.ToString();

        private const string SasToken = "<SAS token>";

        private static readonly EasClientDeviceInformation DeviceInfo;

        static StartupTask()
        {
            DeviceInfo=new EasClientDeviceInformation();
        }

        public StartupTask()
        {
            IAuthenticationMethod auth = new DeviceAuthenticationWithToken(DeviceId, SasToken);
            _deviceClient = DeviceClient.Create(Hostname, auth);
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                Doorphone doorphone = new Doorphone(new IPAddress(new byte[] {192, 168, 1, 250}));
                doorphone.BoldEvent += Doorphone_BoldEvent;
                await doorphone.InitializeAsync();
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
            catch (Exception ex)
            {
            }
        }

        private async void Doorphone_BoldEvent(object sender, BoldEventHandlerArgs args)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(args));
            Message message = new Message(data);
            await _deviceClient.SendEventAsync(message);
        }
    }
}