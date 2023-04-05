using System.Text;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;

namespace SocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var ipep = new IPEndPoint(IPAddress.Parse("192.168.10.10"), 20000);
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                client.Connect(ipep);
                new Task(() =>
                {
                    try
                    {
                        while (true)
                        {
                            var binary = new Byte[1024];
                            client.Receive(binary);
                            var data = Encoding.ASCII.GetString(binary).Trim('\0');
                            if (String.IsNullOrWhiteSpace(data))
                            {
                                continue;
                            }
                            Console.Write(data);
                        }
                    }
                    catch (SocketException)
                    {
                    }
                }).Start();

                while (true)
                {
                    var msg = Console.ReadLine();
                    client.Send(Encoding.ASCII.GetBytes(msg + "\r\n"));
                    if ("EXIT".Equals(msg, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }
                Console.WriteLine($"Disconnected");
            }
            Console.WriteLine("Press Any key...");
            Console.ReadLine();
        }
    }
}
