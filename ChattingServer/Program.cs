using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChattingServer
{
    class ChattingSession : Session
    {
        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string message = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            foreach (var socket in Program.clientSockets)
                socket.Send(new ArraySegment<byte>(buffer.Array, buffer.Offset, buffer.Count));
            Console.WriteLine(message);
            return buffer.Count;
        }

        public override void OnSend(ArraySegment<byte> buffer)
        {
        }
    }
    class Program
    {
        static Listener _listener = new Listener();
        public static List<Session> clientSockets = new List<Session>();
        static ManageDatabase dbManager = new ManageDatabase();
        private static bool _isGetName = false;
        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 3001);
            Console.WriteLine("DBIp: ");
            string DBIp = Console.ReadLine(); 
            Console.WriteLine("DBName: ");
            string DBName = Console.ReadLine();
            Console.WriteLine("PortNum: ");
            int portNum = int.Parse(Console.ReadLine());
            Console.WriteLine("Password: ");
            string password = Console.ReadLine();
            Console.WriteLine("UserId: ");
            string uid = Console.ReadLine();
            Console.WriteLine("TableName: ");
            string tableName = Console.ReadLine();
            dbManager.ConnectDB($"Server={DBIp};Port={portNum};Database={DBName};Uid={uid};Pwd={password}",tableName);
            try
            {
                _listener.Init(endPoint, () => { return new ChattingSession(); }, (Session session) => { clientSockets.Add(session); }, isGetName);
                Console.WriteLine("서버 오픈");
                Socket beforeSocket = null;
                while (true)
                {
                    Socket socket = _listener.clientSocket;
                    if (socket != null && beforeSocket != socket)
                    {
                        _isGetName = false;
                        string clientIp = socket.RemoteEndPoint.ToString().Split(':')[0];
                        if (!dbManager.SearchDuplication(clientIp))
                        {
                            Task.Run(() => { ReceiveName(socket, clientIp); });
                        }
                        else
                        {
                            Task.Run(() => { SendName(socket, clientIp); });
                        }
                        beforeSocket = socket;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static bool isGetName()
        {
            while (true)
            {
                if (_isGetName) return true;
            }
        }

        private static void ReceiveName(Socket clientSocket, string ip)
        {
            Console.WriteLine("Insert");
            byte[] buffer = new byte[15];
            clientSocket.Receive(buffer);
            Console.WriteLine("RecvSync");
            string name = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            dbManager.InsertClientInfo(ip, name);
            SendName(clientSocket, ip);
        }
        private static void SendName(Socket clientSocket, string ip)
        {
            string name = dbManager.GetClientName(ip);
            byte[] buffer = Encoding.UTF8.GetBytes(name);
            clientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            _isGetName = true;
        }
    }
}
