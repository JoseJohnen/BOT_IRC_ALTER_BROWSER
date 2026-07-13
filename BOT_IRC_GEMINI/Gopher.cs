namespace BOT_IRC_GEMINI;

using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

public class Gopher
{
    public static string _pth = "gopherRoot";
    private static string _prt = "7070";
    private static string _lch = "localhost";
    private static string _cert1 = "cert.pem";
    private static string _cert2 = "key.pem";

    public static async void StartGopherServer(string[] args = null)
    {
        if (args != null && args.Length >= 2)
        {
            _pth = args[0];
            _prt = args[1];
            _lch = args[2];
        }

        Console.WriteLine($"Usage: rootdir port (e.g. ./root 70)");

        string root = Path.GetFullPath(_pth);
        int port = int.Parse(_prt);

        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Console.WriteLine($"Gopher on {port}");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();

            _ = Task.Run(async () =>
            {
                using TcpClient tcp = client;
                try
                {
                    using NetworkStream stream = tcp.GetStream();
                    using StreamReader reader = new StreamReader(stream);
                    using StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

                    string? request = await reader.ReadLineAsync();

                    // Gopher envía un selector (ruta), si está vacío se considera la raíz "/"
                    string selector = string.IsNullOrWhiteSpace(request) ? "" : request.Trim();

                    selector = selector.TrimStart('/').Replace('\\', '/');
                    string targetPath = Path.Combine(root, selector).Replace("\r", "").Replace("\n", "");

                    //1. CARGAR BIENVENIDA.TXT DINÁMICAMENTE EN LA RAÍZ
                    // Si el selector está vacío (estamos en la raíz) y el archivo bienvenida.txt existe
                    if (Directory.Exists(targetPath) && !targetPath.Contains("gopherRoot/"))
                    {
                        string welcomeFilePath = Path.Combine(root, "bienvenida.txt");
                        if (string.IsNullOrEmpty(selector) && File.Exists(welcomeFilePath))
                        {
                            // Leemos todas las líneas del archivo físico
                            string[] welcomeLines = await File.ReadAllLinesAsync(welcomeFilePath);

                            foreach (string line in welcomeLines)
                            {
                                // Si la línea está vacía, la servimos como una línea de información en blanco
                                if (string.IsNullOrWhiteSpace(line))
                                {
                                    await writer.WriteLineAsync("i\t\tfalse\t0");
                                    continue;
                                }

                                // Detectamos si la línea ya es un enlace Gopher válido (ej: empieza con 0, 1, 7, etc.)
                                char firstChar = line[0];
                                bool isGopherLink = char.IsDigit(firstChar);

                                if (isGopherLink)
                                {
                                    // Enviamos la línea del enlace exactamente como está escrita en el txt
                                    await writer.WriteLineAsync(line);
                                }
                                else
                                {
                                    // Si es texto normal, le añadimos el prefijo 'i' obligatorio para menús
                                    await writer.WriteLineAsync($"i{line}\t\tfalse\t0");
                                }
                            }

                            // Línea en blanco decorativa para separar la bienvenida de los archivos/carpetas
                            await writer.WriteLineAsync("i\t\tfalse\t0");
                        }

                        // 2. ESCANEAR LA CARPETA HERMANA "documentos"
                        string[] directories = Directory.GetDirectories(root);
                        if (directories.Length > 0)
                        {
                            await writer.WriteLineAsync("i--- ARCHIVOS DISPONIBLES ---\t\tfalse\t0");

                            string folderPath = string.Empty;
                            foreach (string str in directories)
                            {
                                folderPath = Path.Combine(root, str);
                                if (Directory.Exists(folderPath))
                                {
                                    string[] archives = Directory.GetFiles(folderPath);
                                    foreach (string archivo in archives)
                                    {
                                        string nombreArchivo = Path.GetFileName(archivo);

                                        // Creamos el selector virtual que el cliente enviará de vuelta
                                        string selectorArchivo = $"/documentos/{nombreArchivo}";


                                        // Tipo 0 porque asumimos que son archivos .txt planos
                                        await writer.WriteLineAsync(
                                            $"0Ver {nombreArchivo}\t{selectorArchivo}\t{_lch}\t{_prt}");
                                    }
                                }
                            }
                        }

                        // Recuerda cerrar siempre el menú Gopher con un punto solo al final de la respuesta tcp
                        await writer.WriteLineAsync(".");
                    }
                    else if (Directory.Exists(targetPath))
                    {
                        string[] directories = Directory.GetDirectories(root);
                        if (directories.Length > 0)
                        {
                            await writer.WriteLineAsync("i--- ARCHIVOS DISPONIBLES ---\t\tfalse\t0");

                            string folderPath = string.Empty;
                            foreach (string str in directories)
                            {
                                folderPath = Path.Combine(root, str);
                                if (Directory.Exists(folderPath))
                                {
                                    string[] archives = Directory.GetFiles(folderPath);
                                    foreach (string archivo in archives)
                                    {
                                        string nombreArchivo = Path.GetFileName(archivo);

                                        // Creamos el selector virtual que el cliente enviará de vuelta
                                        string selectorArchivo = $"/documentos/{nombreArchivo}";


                                        // Tipo 0 porque asumimos que son archivos .txt planos
                                        await writer.WriteLineAsync(
                                            $"0Ver {nombreArchivo}\t{selectorArchivo}\t{_lch}\t{_prt}");
                                    }
                                }
                            }
                        }

                        // Recuerda cerrar siempre el menú Gopher con un punto solo al final de la respuesta tcp
                        await writer.WriteLineAsync(".");
                    }
                    else if (File.Exists(targetPath))
                    {
                        string welcomeFilePath = Path.Combine(root, targetPath);
                        if (File.Exists(welcomeFilePath))
                        {
                            // Leemos todas las líneas del archivo físico
                            string[] welcomeLines = await File.ReadAllLinesAsync(welcomeFilePath);

                            foreach (string line in welcomeLines)
                            {
                                // Si la línea está vacía, la servimos como una línea de información en blanco
                                if (string.IsNullOrWhiteSpace(line))
                                {
                                    await writer.WriteLineAsync("i\t\tfalse\t0");
                                    continue;
                                }

                                // Detectamos si la línea ya es un enlace Gopher válido (ej: empieza con 0, 1, 7, etc.)
                                char firstChar = line[0];
                                bool isGopherLink = char.IsDigit(firstChar);

                                if (isGopherLink)
                                {
                                    // Enviamos la línea del enlace exactamente como está escrita en el txt
                                    await writer.WriteLineAsync(line);
                                }
                                else
                                {
                                    // Si es texto normal, le añadimos el prefijo 'i' obligatorio para menús
                                    await writer.WriteLineAsync($"i{line}\t\tfalse\t0");
                                }
                            }

                            // Recuerda cerrar siempre el menú Gopher con un punto solo al final de la respuesta tcp
                            await writer.WriteLineAsync(".");
                        }
                    }
                    else
                    {
                        await writer.WriteLineAsync("3Error: Item not found.");
                    }

                    // Gopher cierra la conexión al finalizar la respuesta
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling client: {ex.Message}");
                }
            });
        }
    }
}