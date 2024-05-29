using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TcpListenerApp
{
    public class Program
    {

        private static void Main()
        {
            TcpServerAsync().Wait();
        }

        private static async Task TcpServerAsync()
        {
            IPAddress ip;
            var oplexServerAddress = Environment.GetEnvironmentVariable("OPLEX_SERVER");

            if (!IPAddress.TryParse(ConfigurationManager.AppSettings["ipAddress"], out ip))
            {
                Console.WriteLine("Failed to get IP address, service will listen for client activity on all network interfaces.");
                ip = IPAddress.Any;
            }

            int port;
            if (!int.TryParse(ConfigurationManager.AppSettings["port"], out port))
            {
                throw new ArgumentException("Port is not valid.");
            }

            Console.WriteLine("Starting listener...");
            var server = new TcpListener(ip, port);
            server.Start();
            Console.WriteLine("Listening...");
            Console.WriteLine("Starting HttpClient...");

            var httpClient = new OplexClient(oplexServerAddress);

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                var cw = new TcpClientService(client, httpClient);
                ThreadPool.UnsafeQueueUserWorkItem(x => ((TcpClientService) x).Run(), cw);
            }
        }
    }
}