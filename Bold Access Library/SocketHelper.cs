using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Microsoft.BAL
{
    public static class SocketHelper
    {
        public static Task<SocketAsyncEventArgs> ConnectAsync(Socket socket, EndPoint endPoint)
        {
            TaskCompletionSource<SocketAsyncEventArgs> tcs = new TaskCompletionSource<SocketAsyncEventArgs>();
            SocketAsyncEventArgs asyncEventArgs = new SocketAsyncEventArgs
            {
                RemoteEndPoint = endPoint
            };
            asyncEventArgs.Completed += (sender, args) =>
            {

                tcs.SetResult(args);
            };
            asyncEventArgs.UserToken = socket;
            bool isPending = socket.ConnectAsync(asyncEventArgs);
            if (!isPending)
            {
                tcs.SetResult(asyncEventArgs);
            }
            return tcs.Task;
        }

        public static Task<SocketAsyncEventArgs> SendAsync(Socket socket, byte[] data)
        {
            TaskCompletionSource<SocketAsyncEventArgs> tcs = new TaskCompletionSource<SocketAsyncEventArgs>();
            SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
            saea.SetBuffer(data, 0, data.Length);
            saea.UserToken = socket;
            saea.Completed += (sender, args) =>
            {
                tcs.SetResult(args);
            };
            bool isPending = socket.SendAsync(saea);
            if (!isPending)
            {
                tcs.SetResult(saea);
            }
            return tcs.Task;
        }
        public static Task<SocketAsyncEventArgs> ReceiveAsync(Socket socket)
        {
            TaskCompletionSource<SocketAsyncEventArgs> tcs = new TaskCompletionSource<SocketAsyncEventArgs>();
            SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
            byte[] data = new byte[65536];
            saea.SetBuffer(data, 0, data.Length);
            saea.UserToken = socket;
            saea.Completed += (sender, args) =>
            {
                tcs.SetResult(args);
            };
            bool isPending = socket.ReceiveAsync(saea);
            if (!isPending)
            {
                tcs.SetResult(saea);
            }
            return tcs.Task;
        }
    }
}