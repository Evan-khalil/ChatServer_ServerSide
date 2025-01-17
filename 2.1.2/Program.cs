using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace _2._1._2
{
    internal class Program
    {
        private static int Port;
        private static readonly List<TcpClient> OnlineClients = new List<TcpClient>();
        private static void Main(string[] args)
        {
            Console.WriteLine("Ange Port eller klick för att starta servern på default port(2000)");
            string readValue = Console.ReadLine();
            if (int.TryParse(readValue, out int outValue))
            {
                Port = int.Parse(readValue);
            }
            else
            {
                Port = 2000;
            }
            TcpListener tcpListener = new TcpListener(Port);
            Thread _thread = new Thread(new ParameterizedThreadStart(NewServer));
            _thread.Start(tcpListener);
        }
        public static void NewServer(object obj)
        {
            TcpListener server = (TcpListener)obj;
            server.Start();
            while (true)
            {
                Console.Write("Host: " + GetLocalIPAddress() + "\r\n");
                Console.Write("Port: " + Port + "\r\n");
                Console.WriteLine("Online clients:" + OnlineClients.Count + "\r\n");
                Console.Write("Waiting for a connection... \r\n");
                TcpClient tcpClient = server.AcceptTcpClientAsync().Result;
                for (int j = OnlineClients.Count - 1; j >= 0; j--)
                {
                    if (SocketConnected(OnlineClients[j].Client) == false)
                    {
                        Console.WriteLine("Client " + GetIpAddress(OnlineClients[j]) + " has been disconnected \r\n");
                        OnlineClients.RemoveAt(j);
                        Console.WriteLine("Online clients:" + OnlineClients.Count + "\r\n");
                    }
                }
                Console.WriteLine("Client connected with IP: " + GetIpAddress(tcpClient));
                OnlineClients.Add(tcpClient);
                Thread _thread = new Thread(new ParameterizedThreadStart(NewClient));
                _thread.Start(tcpClient);

            }
        }
        public static void NewClient(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
            while (obj != null)
            {
                byte[] bytesBuffer = new byte[256];
                int i;
                try
                {

                    while ((i = tcpClient.GetStream().Read(bytesBuffer, 0, bytesBuffer.Length)) != 0)
                    {
                        string recievedData = GetIpAddress(tcpClient) + " : " + Encoding.ASCII.GetString(bytesBuffer, 0, i);
                        byte[] sendBytes = Encoding.ASCII.GetBytes(recievedData);
                        Console.WriteLine(recievedData);
                        foreach (TcpClient client in OnlineClients)
                        {
                            client.GetStream().Write(sendBytes, 0, sendBytes.Length);
                            client.GetStream().Flush();
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        public static string GetLocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("an error occured");
        }
        public static bool SocketConnected(Socket s)
        {
            bool canRead = s.Poll(1000, SelectMode.SelectRead);
            bool dataAvailable = (s.Available == 0);
            if (canRead & dataAvailable)
            {
                return false;
            }
            return true;
        }
        public static string GetIpAddress(TcpClient tcpClient)
        {
            return ((IPEndPoint)tcpClient.Client.LocalEndPoint).Address.ToString();
        }
    }
}
