using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace BOT_IRC_ALTER_BROWSER;

public class Pirate
{
    public static string _pth = "pirateRoot";
    public static string _prt = "1313";
    public static string _cert1 = "cert.pem";
    public static string _cert2 = "key.pem";

    public static async void StartPirateServer(string[] args = null)
    {
        if (args != null)
        {
            if (args.Length >= 4)
            {
                _pth = args[0];
                _prt = args[1];
                _cert1 = args[2];
                _cert2 = args[3];
            }
        }

        Console.WriteLine("Usage: rootdir port cert.pem key.pem");

        string root = Path.GetFullPath(_pth);
        int port = int.Parse(_prt);

        X509Certificate2 cert = X509Certificate2.CreateFromPemFile(_cert1, _cert2);

        TcpListener listener = new TcpListener(IPAddress.Any, port);

        listener.Start();

        Console.WriteLine($"Pirate on {port}");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();

            _ = Task.Run(async () =>
            {
                using TcpClient tcp = client;

                try
                {
                    using SslStream ssl =
                        new SslStream(tcp.GetStream());

                    await ssl.AuthenticateAsServerAsync(
                        cert,
                        false,
                        SslProtocols.Tls12 | SslProtocols.Tls13,
                        false);

                    using StreamReader reader =
                        new StreamReader(ssl);

                    string? request =
                        await reader.ReadLineAsync();

                    if (request == null)
                        return;

                    Uri uri;

                    try
                    {
                        uri = new Uri(request);
                    }
                    catch
                    {
                        await Write(ssl, "27\r\n"); //Bad Request
                        return;
                    }

                    if (uri.Scheme != "pirate")
                    {
                        await Write(ssl, "27\r\n"); //53 Proxy Request Refused but for Pirate Protocol, that is the equivalent of a Bad request
                        return;
                    }

                    string path =
                        Path.Combine(
                            root,
                            uri.AbsolutePath.TrimStart('/'));

                    if (!path.EndsWith(".prt"))
                        path = Path.Combine(
                            path,
                            "index.prt");

                    path = Path.GetFullPath(path);

                    if (!path.StartsWith(root))
                    {
                        await Write(ssl, "27\r\n"); //Bad Request
                        return;
                    }

                    if (!File.Exists(path))
                    {
                        await Write(ssl, "202\r\n"); //Not Implemented/Not Found
                        return;
                    }

                    await Write(
                        ssl,
                        "1 text/pirate\r\n");

                    byte[] bytes =
                        await File.ReadAllBytesAsync(path);

                    await ssl.WriteAsync(bytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        static Task Write(
            Stream s,
            string text)
        {
            return s.WriteAsync(
                    System.Text.Encoding.UTF8
                        .GetBytes(text))
                .AsTask();
        }
    }
}