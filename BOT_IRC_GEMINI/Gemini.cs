using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace BOT_IRC_GEMINI;

public class Gemini
{
    public static string _pth = "root";
    private static string _prt = "1965";
    private static string _cert1 = "cert.pem";
    private static string _cert2 = "key.pem";

    public static string IRCURI = string.Empty;
        
    public static async void StartGeminiServer(string[] args = null)
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

        Console.WriteLine($"Gemini on {port}");

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

                    // await ssl.AuthenticateAsServerAsync(
                    //     cert,
                    //     false,
                    //     SslProtocols.Tls12 | SslProtocols.Tls13,
                    //     false);

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
                        await Write(ssl, "59\r\n");
                        return;
                    }

                    if (uri.Scheme != "gemini")
                    {
                        await Write(ssl, "53\r\n");
                        return;
                    }

                    string path =
                        Path.Combine(
                            root,
                            uri.AbsolutePath.TrimStart('/'));

                    if (!path.EndsWith(".gmi"))
                        path = Path.Combine(
                            path,
                            "index.gmi");

                    path = Path.GetFullPath(path);

                    if (!path.StartsWith(root))
                    {
                        await Write(ssl, "59\r\n");
                        return;
                    }

                    if (!File.Exists(path))
                    {
                        await Write(ssl, "51\r\n");
                        return;
                    }

                    await Write(
                        ssl,
                        "20 text/gemini\r\n");

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