using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Server
{
    private TcpListener tcpListener;
    private Dictionary<string, List<TcpClient>> clientsByUserName;
    int checker = 0;

    public Server()
    {
        this.tcpListener = new TcpListener(IPAddress.Any, 8888);
        this.clientsByUserName = new Dictionary<string, List<TcpClient>>();
    }

    public void Start()
    {
        this.tcpListener.Start();
        Console.WriteLine("Server started. Waiting for connections...");

        Task.Run(() => ConsoleWriterTask()); // Konsol girişi için yeni bir Task başlatılıyor

        while (true)
        {
            TcpClient client = this.tcpListener.AcceptTcpClient();
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[4096];
        int bytesRead;
        string userName = GetUniqueUserName(client);

        Console.WriteLine("Client connected: " + userName);

        SendMessageToClient(client, "Server: Connection established as " + userName + Environment.NewLine);

        while (true)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine(userName + ": " + message);

                if (message.ToLower() == "exit")
                {
                    RemoveClient(client, userName);
                    break;
                }

                SendMessageToClients(userName + ": " + message);
            }
            catch
            {
                RemoveClient(client, userName);
                break;
            }
        }

        stream.Close();
        client.Close();
    }

    private string GetUniqueUserName(TcpClient client)
    {
        string userName = Environment.UserName;
        int count = 1;
        string originalUserName = userName;

        while (clientsByUserName.ContainsKey(userName))
        {
            userName = originalUserName + count.ToString();
            count++;
        }

        if (!clientsByUserName.ContainsKey(userName))
        {
            clientsByUserName[userName] = new List<TcpClient>();
        }

        clientsByUserName[userName].Add(client);

        return userName;
    }

    private void RemoveClient(TcpClient client, string userName)
    {
        if (clientsByUserName.ContainsKey(userName))
        {
            clientsByUserName[userName].Remove(client);

            if (clientsByUserName[userName].Count == 0)
            {
                clientsByUserName.Remove(userName);
            }
        }

        Console.WriteLine("Client disconnected: " + userName);
    }

    private void SendMessageToClient(TcpClient client, string message)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = Encoding.ASCII.GetBytes(message);
        stream.Write(buffer, 0, buffer.Length);
        stream.Flush();
    }

    private void SendMessageToClients(string message)
    {
        foreach (var clientList in clientsByUserName.Values)
        {
            foreach (TcpClient client in clientList)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
        }
    }

    private void ConsoleWriterTask()
    {
        while (true)
        {
            string input = Console.ReadLine();

            if (input.ToLower() == "exit")
            {
                // Eğer "exit" komutu girildiyse işlemi sonlandır
                break;
            }

            else if (input.ToLower() == "listclients")
            {
                ListConnectedClients();

            }

            else if (input.ToLower() == "clear")
            {
                Console.Clear();
            }

            else
            {
                checker = 1;
            }



            if (checker == 1)
            {
                string[] spl = input.Split('|');

                try
                {
                    SendMessageToSpecificClient(spl[0], spl[1]);

                    checker = 0;

                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("To clear the console screen: clear");


                    checker = 0;
                }
            }
        }
    }

    private void SendMessageToSpecificClient(string userName, string message)
    {
        if (clientsByUserName.ContainsKey(userName))
        {
            List<TcpClient> clientList = clientsByUserName[userName];

            foreach (TcpClient client in clientList)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
        }
    }

    private void ListConnectedClients()
    {
        Console.WriteLine("Connected Clients:");

        int clientIndex = 1;

        foreach (var userName in clientsByUserName.Keys)
        {
            Console.WriteLine($"=> Client{clientIndex}: {userName}");
            clientIndex++;
        }
    }

    public static void Main()
    {
        Server server = new Server();
        server.Start();

        while (true)
        {
            string command = Console.ReadLine();

            if (command.ToLower() == "exit")
            {
                // Eğer "exit" komutu girildiyse işlemi sonlandır
                break;
            }

            else if (command.ToLower() == "listclients")
            {
                server.ListConnectedClients();
            }

            string[] spl = command.Split('|');
            try
            {
                server.SendMessageToSpecificClient(spl[1], spl[2]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}