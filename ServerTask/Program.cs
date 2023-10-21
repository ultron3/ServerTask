using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Server
{
    private const string _ipAddress = "192.168.1.114";
    private const int _port = 5500;
    static List<TcpClient> connectedClients = new List<TcpClient>();

    static async Task Main(string[] args)
    {
        // Impostazione del token di cancellazione
        var cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        // Specifica l'indirizzo IP e la porta su cui il server ascolta
        IPAddress ipAddress = IPAddress.Parse(_ipAddress);
     

        // Avvio del server in un thread separato
        Task serverTask = StartServerAsync(ipAddress, _port, token);
     
        Console.WriteLine($"Server in ascolto su {ipAddress}:{_port}.");

        PrintConnectedClients();

        // Richiedi l'annullamento del server e attendi che si fermi

        Console.ReadLine();
        cts.Cancel();
        await serverTask;

    }

    static async Task StartServerAsync(IPAddress ipAddress, int port, CancellationToken token)
    {
        var listener = new TcpListener(ipAddress, port);
        listener.Start();

        while (!token.IsCancellationRequested)
        {
            try
            {
                TcpClient client = await listener.AcceptTcpClientAsync();

                // Gestione della connessione client in un nuovo task
                Task.Run(() => HandleClientAsync(client, token));
            }
            catch (ObjectDisposedException)
            {
                // Listener è stato chiuso
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'accettazione del client: {ex.Message}");
            }
        }

        listener.Stop();
    }

    static async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        // Implementazione della gestione del client come prima
        using (client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            while (!token.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                if (bytesRead == 0)
                {
                    // La connessione è stata chiusa
                    break;
                }

                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string response = $"Ricevuto: {request}";

                byte[] responseData = Encoding.UTF8.GetBytes(response);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
             
                Console.WriteLine($"Messaggio da {((IPEndPoint)client.Client.RemoteEndPoint).Address}:{((IPEndPoint)client.Client.RemoteEndPoint).Port}: {message}");
                await stream.WriteAsync(responseData, 0, responseData.Length, token);
            }
        }

       
    }
    static void PrintConnectedClients()
    {
        Console.WriteLine("Client connessi:");
        foreach (TcpClient client in connectedClients)
        {
            Console.WriteLine($"{((IPEndPoint)client.Client.RemoteEndPoint).Address}:{((IPEndPoint)client.Client.RemoteEndPoint).Port}");
        }
    }
}


