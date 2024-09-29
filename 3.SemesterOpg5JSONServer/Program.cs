using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    public static void Main()
    {
        Console.WriteLine("TCP Server:");

        TcpListener listener = new TcpListener(IPAddress.Any, 8000);
        listener.Start();

        while (true)
        {
            TcpClient socket = listener.AcceptTcpClient();
            IPEndPoint clientEndPoint = socket.Client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine("Client connected: " + clientEndPoint.Address);

            Task.Run(() => HandleClient(socket));
        }

        listener.Stop();
    }

    static void HandleClient(TcpClient socket)
    {
        using NetworkStream ns = socket.GetStream();
        using StreamReader reader = new StreamReader(ns);
        using StreamWriter writer = new StreamWriter(ns) { AutoFlush = true };

        while (socket.Connected)
        {
            string jsonMessage = reader.ReadLine();
            if (string.IsNullOrEmpty(jsonMessage))
                break;

            Console.WriteLine("Received: " + jsonMessage);
            var response = ProcessRequest(jsonMessage);
            writer.WriteLine(JsonSerializer.Serialize(response));
        }
        socket.Close();
    }

    static object ProcessRequest(string jsonMessage)
    {
        try
        {
            var request = JsonSerializer.Deserialize<Request>(jsonMessage);
            if (request == null) throw new Exception("Invalid request");

            switch (request.Method.ToLower())
            {
                case "random":
                    return new Response { Result = new Random().Next(request.Number1, request.Number2 + 1), Message = "Random number generated." };

                case "add":
                    return new Response { Result = request.Number1 + request.Number2, Message = "Addition performed." };

                case "subtract":
                    return new Response { Result = request.Number1 - request.Number2, Message = "Subtraction performed." };

                default:
                    return new Response { Error = "Unknown method" };
            }
        }
        catch (Exception ex)
        {
            return new Response { Error = ex.Message };
        }
    }
}

public class Request
{
    public string Method { get; set; }
    public int Number1 { get; set; }
    public int Number2 { get; set; }
}

public class Response
{
    public int? Result { get; set; }
    public string Message { get; set; }
    public string Error { get; set; }
}
