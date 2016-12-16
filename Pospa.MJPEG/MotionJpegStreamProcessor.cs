using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace Pospa.MJPEG
{
    public class MotionJpegStreamProcessor
    {
        private readonly Uri _mjpegStreamUri;
        public event ImageEventHandler BoldEvent;

        protected virtual void OnBoldEvent(ImageEventHandlerArgs args)
        {
            BoldEvent?.Invoke(this, args);
        }

        public MotionJpegStreamProcessor(Uri mjpegStreamUri)
        {
            _mjpegStreamUri = mjpegStreamUri;
        }

        public async Task StartListeningAsync()
        {
            HttpWebRequest request = WebRequest.CreateHttp(_mjpegStreamUri);
            WebResponse response = await request.GetResponseAsync();
            Stream responseStream = response.GetResponseStream();
            using (MultipartStreamReader streamReader = new MultipartStreamReader(responseStream))
            {
                while (!streamReader.IsEndOfStream)
                {
                    
                }
            }
        }

        public async Task StopListeningAsync()
        {
        }
    }

    public delegate void ImageEventHandler(object sender, ImageEventHandlerArgs args);

    public class ImageEventHandlerArgs
    {
        public readonly Windows.Graphics.Imaging.ImageStream ImageStream;

        public ImageEventHandlerArgs(ImageStream imageStream)
        {
            ImageStream = imageStream;
        }
    }
}
