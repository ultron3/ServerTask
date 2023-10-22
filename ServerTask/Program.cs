using System;
using System.Collections.Generic;
using System.IO;
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
    private static TcpClient clientObj;

    static async Task Main(string[] args)
    {
        // Impostazione del token di cancellazione
        var cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        // Specifico l'indirizzo IP e la porta su cui il server ascolta
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
                // Verifica se i dati sono un messaggio o un file
                if (IsTextMessage(buffer, bytesRead))
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Messaggio da {((IPEndPoint)client.Client.RemoteEndPoint).Address}:{((IPEndPoint)client.Client.RemoteEndPoint).Port}: {message}");
                }
                else
                {
                    // Se non è un messaggio, considera i dati come un file
                    string fileName = $"file_{DateTime.Now:yyyyMMddHHmmss}.dat";
                    SaveFile(buffer, bytesRead, fileName);
                    Console.WriteLine($"Ricevuto file da {((IPEndPoint)client.Client.RemoteEndPoint).Address}:{((IPEndPoint)client.Client.RemoteEndPoint).Port}: {fileName}");
                }

               
            }
        }

       
    }
    static bool IsTextMessage(byte[] data, int length)
    {
      
        // In questo esempio, consideriamo messaggio se i dati contengono testo UTF-8.
        try
        {
            Encoding.UTF8.GetString(data, 0, length);
            return true;
        }
        catch
        {
            return false;
        }
    }

    static void SaveFile(byte[] data, int length, string fileName)
    {

        string directoryPath = "C:\\Users\\IdeaPad\\OneDrive\\Documenti"; 

        // bisogna assicurarsi che la directory esista altrimenti la si crea
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, fileName);

        using (FileStream fs = File.Create(filePath))
        {
            fs.Write(data, 0, length);
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


