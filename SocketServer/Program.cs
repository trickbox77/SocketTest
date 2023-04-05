using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace Example
{
    class Program
    {
        static async Task RunServer(int port)
        {
            var ipep = new IPEndPoint(new IPAddress(new byte[] { 192, 168, 10, 10 }), port);
			using Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			server.Bind(ipep);
			server.Listen(20);
			Console.WriteLine($"Server Start... Listen port {ipep.Port}...");
			var task = new Task(() =>
			{
				while (true)
				{
					var client = server.Accept();
					new Task(() =>
					{
						var ip = client.RemoteEndPoint as IPEndPoint;
						Console.WriteLine($"Client : (From: {ip.Address.ToString()}:{ip.Port}, Connection time: {DateTime.Now})");
						client.Send(Encoding.ASCII.GetBytes("Welcome server! You connected to 192.168.10.10 : 20000\r\n>"));
						var sb = new StringBuilder();
						using (client)
						{
							while (true)
							{
								var binary = new Byte[1024];
								client.Receive(binary);
								var data = Encoding.ASCII.GetString(binary);
								sb.Append(data.Trim('\0'));
								if (sb.Length > 2 && sb[sb.Length - 2] == '\r' && sb[sb.Length - 1] == '\n')
								{
									data = sb.ToString().Replace("\n", "").Replace("\r", "");
                                    if (String.IsNullOrWhiteSpace(data))
                                    {
                                        continue;
                                    }
                                    if ("EXIT".Equals(data, StringComparison.OrdinalIgnoreCase))
                                    {
                                        break;
                                    }
                                    Console.WriteLine("Message = " + data);
                                    sb.Length = 0;
                                    var sendMsg = Encoding.ASCII.GetBytes("ECHO from server : " + data + "\r\n>");
                                    client.Send(sendMsg);
								}
							}
							Console.WriteLine($"Disconnected : (From: {ip.Address.ToString()}:{ip.Port}, Connection time: {DateTime.Now})");
						}
					}).Start();

					new Task(() =>
					{
						using (client)
						{
							while (true)
							{
								Random rnd = new Random();
								var sendMsg = Encoding.ASCII.GetBytes($"x:{rnd.NextDouble()},y:{rnd.NextDouble()},yaw:{rnd.NextDouble()}\r\n>");
								client.Send(sendMsg);
								Thread.Sleep(1000);
							}
						}
					}).Start();
				}
			});
			task.Start();
			await task;
		}
        static void Main(string[] args)
        {
            RunServer(20000).Wait();
            Console.WriteLine("Press Any key...");
            Console.ReadLine();
        }
    }
}
