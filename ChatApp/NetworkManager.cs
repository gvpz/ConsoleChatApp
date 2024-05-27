using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System;

namespace ChatApp
{
    public class NetworkManager
    {
        private static TcpListener host;
        private static bool connected;
        private static TcpClient client;

        public static async Task Connect(string ipAddr, int port)
        {
            try
            {
                client = new TcpClient();
                Console.WriteLine($"Attempting to connect to {ipAddr}:{port}");
                
                await client.ConnectAsync(ipAddr, port);
                await Program.encryption.SendPublicKey(client);
                await Program.encryption.ReceiveKey(client);
                
                Console.WriteLine("Connected to host. Say hi!");
                connected = true;
                
                //Send public key

                _ = Task.Run(() => ListenForMessages(client));
                await SendMessage(client);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"SocketException: {ex.Message} (ErrorCode: {ex.ErrorCode})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
            }
        }

        public static async Task StartListener(string ipAddr, int port)
        {
            try
            {
                var ip = IPAddress.Any;
                host = new TcpListener(ip, port);
                host.Start();
                host.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                Console.WriteLine($"Listening at {ipAddr}, {port}");
                
                client = await host.AcceptTcpClientAsync();
                await Program.encryption.ReceiveKey(client);
                await Program.encryption.SendPublicKey(client);
                
                Console.WriteLine("Client connected. Say hi!");
                connected = true;
                
                _ = Task.Run(() => ListenForMessages(client));
                await SendMessage(client);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"SocketException: {ex.Message} (ErrorCode: {ex.ErrorCode})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartListener Exception: {ex.Message}");
            }
        }

        public static void StopListener()
        {
            host?.Stop();
            client?.Close();
            connected = false;
        }

        private static async Task ListenForMessages(TcpClient client)
        {
            var stream = client.GetStream();
            var buffer = new byte[1024];

            try
            {
                while (connected)
                {
                    var bytesRead = await stream.ReadAsync(buffer);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    var encryptedData = new byte[bytesRead];
                    Array.Copy(buffer, 0, encryptedData, 0, bytesRead);

                    var message = Program.encryption.DecryptMessage(encryptedData);
                    Console.WriteLine($"Received: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from stream: {ex.Message}");
            }
        }


        private static async Task SendMessage(TcpClient client)
        {
            var stream = client.GetStream();
            while (connected)
            {
                var message = Console.ReadLine();
                if (message == null) continue;

                var buffer = Program.encryption.EncryptMessage(message);
                await stream.WriteAsync(buffer);
            }
        }

        public static bool IsPortInUse(string ipAddr, int port)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return false;
            }
            catch (SocketException)
            {
                return true;
            }
        }

        public static async Task<string> GetIpAddress()
        {
            using var httpClient = new HttpClient();
            try
            {
                string response = await httpClient.GetStringAsync("https://api64.ipify.org?format=json");
                int startIndex = response.IndexOf("\"ip\":\"") + 6;
                int endIndex = response.IndexOf("\"", startIndex);
                return response.Substring(startIndex, endIndex - startIndex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
    }
}
