using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ServerCore
{
    public abstract class Session
    {
        public static void Main(string[] Args)
        {
        }
        Socket _socket;

        object _locker = new object();

        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        private RecvBuffer _recvBuffer = new RecvBuffer(4096);

        private Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        private List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(ArraySegment<byte> buffer);

        public void Init(Socket socket)
        {
            _socket = socket;
            Console.WriteLine(_socket.RemoteEndPoint);
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterReceive();
        }
        #region Send
        public void Send(ArraySegment<byte> buffer)
        {
            lock (_locker)
            {
                _sendQueue.Enqueue(buffer);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        private void RegisterSend()
        {
            while (_sendQueue.Count > 0)
                _pendingList.Add(_sendQueue.Dequeue());

            _sendArgs.BufferList = _pendingList.ToArray();
            bool pending = _socket.SendAsync(_sendArgs);
            if (!pending)
                OnSendCompleted(null, _sendArgs);
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_locker)
            {
                if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
                {
                    try
                    {
                        args.BufferList = null;
                        _pendingList.Clear();

                        if (_sendQueue.Count > 0)
                            RegisterSend();
                    }
                    catch
                    {
                        Console.WriteLine("error");
                    }
                }
            }
        }
        #endregion
        #region Receive
        private void RegisterReceive()
        {
            _recvBuffer.Clear();
            ArraySegment<byte> writeSegment = _recvBuffer.RecvSegment;
            _recvArgs.SetBuffer(writeSegment.Array, writeSegment.Offset, writeSegment.Count);
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (!pending)
                OnRecvCompleted(null, _recvArgs);
        }

        private void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    //Write cursur move
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        return;
                    }

                    //컨텐츠쪽으로 데이터 넘기고 얼마나 처리했는지 받는다
                    int len = OnRecv(_recvBuffer.DataSegment);
                    if (len < 0 || _recvBuffer.DataSize < len)
                    {
                        Console.WriteLine("Error");
                        return;
                    }
                    if (_recvBuffer.OnRead(len) == false)   
                    {
                        Console.WriteLine(len);
                        return;
                    }
                    RegisterReceive();//다시 비동기로 받아줌
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
        #endregion 
    }
}
