using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public class Connector
    {
        private Func<Session> _sessionFactory;
        public Session session;

        public void Initialize(IPEndPoint endPoint, Func<Session> factory)
        {
            _sessionFactory = factory;
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;

            RegisterConnect(args);
        }

        private void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null) return;
            bool pending = socket.ConnectAsync(args);
            if (!pending)
                OnConnectCompleted(null, args);

        }

        private void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Console.WriteLine("ConnnectComplete");
                Session session = _sessionFactory?.Invoke();
                session.Init(args.ConnectSocket);
                Console.WriteLine("Init Server Socket");
                this.session = session;
            }
        }
    }
}
