using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        public Socket clientSocket { get; private set; }
        Func<Session> _action;
        Action<Session> _returnSession;
        Func<bool> _getName;
        public void Init(IPEndPoint endPoint, Func<Session> action, Action<Session> action1, Func<bool> getName)
        {
            _getName = getName;
            _action = action;
            _returnSession = action1;
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(endPoint);

            _listenSocket.Listen(3);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnAcceptCompleted;

            RegisterAccept(args);
        }

        private void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;
            clientSocket = null;
            bool pending = _listenSocket.AcceptAsync(args);

            if (!pending)
                OnAcceptCompleted(null, args);
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _action.Invoke();
                clientSocket = args.AcceptSocket;
                if (_getName.Invoke())
                {
                    session.Init(args.AcceptSocket);
                    _returnSession?.Invoke(session);
                }
                Console.WriteLine("add client");

                RegisterAccept(args);
            }
        }
    }
}
