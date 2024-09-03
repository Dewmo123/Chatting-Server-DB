using ServerCore;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    class GameSession : Session
    {
        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string message = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            if (Program.name == null)
            {
                Program.name = message;
                Console.WriteLine("SetName");
            }
            Console.WriteLine(message);
            return buffer.Count;
        }

        public override void OnSend(ArraySegment<byte> buffer)
        {
        }
    }
    class Program
    {
        public static string name;

        static void Main(string[] args)
        {
            IPAddress serverIP = IPAddress.Parse(Console.ReadLine());

            IPEndPoint endPoint = new IPEndPoint(serverIP, 3001);
            Connector connector = new Connector();
            connector.Initialize(endPoint, () => { return new GameSession(); });

            byte[] buffer = new byte[1024];
            string message = null;
            while (true)
            {
                message = Console.ReadLine();
                if (message != null)
                {
                    if (name == null)
                        buffer = Encoding.UTF8.GetBytes($"{message}");
                    else
                        buffer = Encoding.UTF8.GetBytes($"{name} : {message}");
                    message = null;
                    connector.session.Send(new ArraySegment<byte>(buffer, 0, buffer.Length));
                }
            }
        }
    }
}
