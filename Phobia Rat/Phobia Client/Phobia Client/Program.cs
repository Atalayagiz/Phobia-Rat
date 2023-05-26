using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    private TcpClient client;
    private NetworkStream stream;
    private bool isConnected;

    public void Start()
    {
        try
        {
            this.client = new TcpClient("127.0.0.1", 8888);
            this.stream = client.GetStream();
            this.isConnected = true;

            Console.WriteLine("Connected to server.");

            // Serverdan gelen mesajları dinleme
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            while (isConnected)
            {
                Console.Write("Enter a message to send to the server: ");
                string message = Console.ReadLine();

                // Client tarafından servera mesaj gönderme
                SendMessageToServer(message);

                if (message.ToLower() == "exit")
                {
                    isConnected = false;
                    break;
                }
            }

            this.stream.Close();
            this.client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    private void ReceiveMessages()
    {
        byte[] buffer = new byte[4096];
        int bytesRead;

        while (isConnected)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received from server: " + message);
            }
            catch
            {
                isConnected = false;
                break;
            }
        }
    }

    private void SendMessageToServer(string message)
    {
        byte[] buffer = Encoding.ASCII.GetBytes(message);
        stream.Write(buffer, 0, buffer.Length);
        stream.Flush();
    }

    static void Main(string[] args)
    {
        Client client = new Client();
        client.Start();
    }
}