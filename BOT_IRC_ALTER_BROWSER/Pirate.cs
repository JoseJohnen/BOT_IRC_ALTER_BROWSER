using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;

namespace BOT_IRC_ALTER_BROWSER;

public class Pirate
{
    public static string _pth = "pirateRoot";
    public static string _prt = "1313";
    public static string _cert1 = "cert.pem";
    public static string _cert2 = "key.pem";

     #region Shock absorbers

    //IT does create "Back Pressure"
    //It will wait for space to be available in order to wait
    private static BoundedChannelOptions options = new BoundedChannelOptions(255);

    private static Channel<string> channelReceive = null;

    public static Channel<string> ChannelReceive
    {
        get
        {
            if (channelReceive == null)
            {
                options.FullMode = BoundedChannelFullMode.Wait;
                channelReceive = System.Threading.Channels.Channel.CreateBounded<string>(options);
            }

            return channelReceive;
        }
        set { channelReceive = value; }
    }

    private static ChannelWriter<string> writerSender = null;

    public static ChannelWriter<string> WriterSender
    {
        get
        {
            if (writerSender == null)
            {
                writerSender = ChannelReceive.Writer;
            }

            return writerSender;
        }
        set => writerSender = value;
    }

    private static ChannelReader<string> writerReceiver = null;

    public static ChannelReader<string> WriterReceiver
    {
        get
        {
            if (writerReceiver == null)
            {
                writerReceiver = ChannelReceive.Reader;
            }

            return writerReceiver;
        }
        set => writerReceiver = value;
    }

    #endregion
    
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
                    using SslStream ssl = new SslStream(tcp.GetStream());

                    await ssl.AuthenticateAsServerAsync(cert, false, SslProtocols.Tls12 | SslProtocols.Tls13, false);

                    using StreamReader reader = new StreamReader(ssl);

                    string? request = await reader.ReadLineAsync();

                    if (request == null)
                    {
                        return;
                    }

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

                    string path = Path.Combine(root, uri.AbsolutePath.TrimStart('/'));

                    if (!path.EndsWith(".prt"))
                    {
                        path = Path.Combine(path, "index.prt");
                    }

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

                    await Write(ssl, "1 text/pirate\r\n");

                    // byte[] bytes = await File.ReadAllBytesAsync(path);
                    string content = await File.ReadAllTextAsync(path);

                    string result = string.Empty;
                    //TODO: Agregar identificador por ID para vincular respuesta con botón correspondiente
                    foreach (string str in content.Split("\n\n",StringSplitOptions.TrimEntries))
                    {
                        result += str+"\n\n";
                        if (str.Contains(">|<"))
                        {
                            while (await WriterReceiver.WaitToReadAsync())
                            {
                                result += await WriterReceiver.ReadAsync();
                                result += "\n\n";
                            }
                        }
                    }
                    
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(result);
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