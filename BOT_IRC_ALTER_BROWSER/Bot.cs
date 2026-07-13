using System.Collections;
using System.Collections.Concurrent;
using System.Net.Security;
using System.Net.Sockets;
using System.Text; // Lo mismo que la anterior
using System.Text.RegularExpressions;
using System.Threading.Channels; // No es vital pero me encantan el uso de las expresiones regulares


namespace BOT_IRC_ALTER_BROWSER;

public class Bot
{
    #region Attributes

    #region Instance Attributes

    public string host = "irc.libera.chat"; // Establecemos la variable string host para tener el host del canal IRC
    public string nickname = "ClapTrakaLaKa"; // Establecemos la variable nickname con el nick del bot
    public string canal = "#locos"; // Establecemos la variable canal con el nombre del canal

    #endregion

    #region Functional Attributes

    private StreamReader leer_datos; // Establecemos la variable leer_datos como StreamReader
    private StreamWriter mandar_datos; // Establecemos la variable mandar_datos como SteamWriter

    private string[] usuarios; // Creamos el string[] usuarios para tener todos los nicks que estan en el canal
    private NetworkStream conexion; // Establecemos la variable conexion como NetworkStream
    private TcpClient irc; // Establecemos la variable irc como TcpClient
    private string code = ""; // Creamos la variable string que vamos a usar para leer los sockets
    private string dedonde;
    private string usuarioCanal;
    private string mensaje;

    private List<Thread> thrGeminiExplorations = new List<Thread>();

    //This saves the user navigational history
    //quien, QueueUrls
    private ConcurrentDictionary<string, ConcurrentStack<string>> cDicUsuarios_Historial =
        new ConcurrentDictionary<string, ConcurrentStack<string>>();

    //This defines where is quien, so to know which list of links use with the user;
    //quien, activeUrl, FechaUltimaAcción
    private ConcurrentDictionary<string, Par<string, DateTime>> cDicListaUsuarios_HiperLinkActivo =
        new ConcurrentDictionary<string, Par<string, DateTime>>();

    //Do not touch, this are the basic functions the bot performs, it filters out
    //any normal comms from actual manual commands
    //Here are the Hiperlinks available to every user
    //Site, HiperLinks, DateOfCreation (to know when to properly ditch them)
    private ConcurrentDictionary<string, List<Trio<string, List<string>, DateTime>>> cDicListaUsuarios_HiperVinculos =
        new ConcurrentDictionary<string, List<Trio<string, List<string>, DateTime>>>();

    //Here are the basic bot functions 
    private Dictionary<string, Func<string, Match, string>> dicBotBasicFuncitions =
        new Dictionary<string, Func<string, Match, string>>();

    //Here you add the commands you want the bot to perform
    private Dictionary<string, Func<string, Match, string>> dicBotExtendedFunctions =
        new Dictionary<string, Func<string, Match, string>>();

    #region Class (static) attributes

    private static CancellationToken _cancelToken = new CancellationToken();
    internal static Thread _thrSendDataIrc = new Thread(() => SendingDataIrc(_cancelToken));

    #endregion

    #endregion

    #endregion

    #region Shock absorbers

    //IT does create "Back Pressure"
    //It will wait for space to be available in order to wait
    private static BoundedChannelOptions options = new BoundedChannelOptions(255);

    private static Channel<Trio<Bot, string, string>> channelReceive = null;

    public static Channel<Trio<Bot, string, string>> ChannelReceive
    {
        get
        {
            if (channelReceive == null)
            {
                options.FullMode = BoundedChannelFullMode.Wait;
                channelReceive = System.Threading.Channels.Channel.CreateBounded<Trio<Bot, string, string>>(options);
            }

            return channelReceive;
        }
        set { channelReceive = value; }
    }

    private static ChannelWriter<Trio<Bot, string, string>> writerSender = null;

    public static ChannelWriter<Trio<Bot, string, string>> WriterSender
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

    private static ChannelReader<Trio<Bot, string, string>> writerReceiver = null;

    public static ChannelReader<Trio<Bot, string, string>> WriterReceiver
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

    public static async void SendingDataIrc(CancellationToken cancellationToken)
    {
        try
        {
            Trio<Bot, string, string> tmpPar = null;
            while (await WriterReceiver.WaitToReadAsync())
            {
                tmpPar = await WriterReceiver.ReadAsync();
                tmpPar.Item1.mandar_datos.WriteLine("PRIVMSG " + tmpPar.Item2 + " : " + tmpPar.Item3);
                tmpPar.Item1.mandar_datos.Flush();
            }
        }
        catch (Exception ex)
        {
            Console.Out.WriteLine($"Error ReadingChannelReceive: {ex.Message}");
        }
    }

    #endregion

    #region Constructors

    public Bot(string host, string nickname, string canal)
    {
        irc = new TcpClient(host, 6667); // Realizamos la conexion con el canal usando el host y el puerto 6667
        conexion = irc.GetStream(); // Cargamos la conexion para poder leer los datos
        leer_datos = new StreamReader(conexion); // Lo necesario para leer los datos de la conexion 
        mandar_datos = new StreamWriter(conexion); // Lo necesario para mandar comandos al canal IRC

        this.host = host;
        this.nickname = nickname;
        this.canal = canal;
    }

    public Bot()
    {
        this.host = host;
        this.nickname = nickname;
        this.canal = canal;
    }

    #endregion

    #region Functions required to establish comms and process the receiving input from IRC

    public void PrepareBot()
    {
        irc = new TcpClient(host, 6667); // Realizamos la conexion con el canal usando el host y el puerto 6667
        conexion = irc.GetStream(); // Cargamos la conexion para poder leer los datos
        leer_datos = new StreamReader(conexion); // Lo necesario para leer los datos de la conexion 
        mandar_datos = new StreamWriter(conexion); // Lo necesario para mandar comandos al canal IRC

        //Do not touch, this are the basic functions the bot performs, it filters out 
        //any normal comms from actual manual commands
        dicBotBasicFuncitions = new Dictionary<string, Func<string, Match, string>>()
        {
            { "PING(.*)", Pong },
            { ":(.*) 353 (.*) @ (.*) :(.*)", ListaUsuarios },
            { ":(.*)!(.*) JOIN (.*)", ProcesarUnionConexionCanal },
            { ":(.*)!(.*) PART (.*)", ProcesarAbandonarConexionCanal },
            { ":(.*)!(.*) PRIVMSG (.*) :(.*)", ProcesarMensajeRegular },
        };

        //Here you add the commands you want the bot to perform
        dicBotExtendedFunctions = new Dictionary<string, Func<string, Match, string>>()
        {
            { "!(.*)d(.*)", DadosDeRol },
            // { "GEMINI:(.*)", FollowLinkGeminiSite },
            { @"\[(.*)\]", FollowLinkSite },
            { "GEMINI>(.*)", FetchGeminiSite },
            { "GOPHER>(.*)", FetchGopherSite },
        };
    }

    public void PrepareBotConection()
    {
        // Instance = this;
        this.mandar_datos.WriteLine("NICK " +
                                    this.nickname); // Usamos el comando NICK para entrar al canal usando el nick antes declarado
        this.mandar_datos.Flush(); // Actualizamos la conexion

        this.mandar_datos.WriteLine("USER " + this.nickname +
                                    " 1 1 1 1"); // Usamos el comando USER para confirmar el nickname
        this.mandar_datos.Flush(); // ..

        this.mandar_datos.WriteLine("JOIN " + this.canal); // Usamos el comando JOIN para entrar al canal
        this.mandar_datos.Flush(); // ..
    }

    public void WorkingBot()
    {
        this.PrepareBot();
        this.PrepareBotConection();
        Match regex = null;
        string result = string.Empty;
        string resultExtended = string.Empty;
        while (true) // Mi bucle enterno
        {
            while ((code = this.leer_datos.ReadLine()) != null) // Leemos la conexion con la variable code
            {
                Console.WriteLine("Code : " + code); // No es necesario pero es para ver las respuestas del servidor

                foreach (KeyValuePair<string, Func<string, Match, string>> valuePair in dicBotBasicFuncitions)
                {
                    if ((regex = Regex.Match(code, valuePair.Key, RegexOptions.IgnoreCase)).Success)
                    {
                        result = valuePair.Value(code, regex);
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            if (result.Contains("°°°"))
                            {
                                result = result.Replace("°°°", "");
                                foreach (KeyValuePair<string, Func<string, Match, string>> valPair in
                                         dicBotExtendedFunctions)
                                {
                                    if ((regex = Regex.Match(result, valPair.Key, RegexOptions.IgnoreCase)).Success)
                                    {
                                        resultExtended = valPair.Value(result, regex);
                                        if (resultExtended.Contains("|n|"))
                                        {
                                            foreach (string strResult in resultExtended.Split("|n|",
                                                         StringSplitOptions.RemoveEmptyEntries))
                                            {
                                                this.mandar_datos.WriteLine(strResult);
                                            }

                                            this.mandar_datos.Flush();
                                            break;
                                        }

                                        this.mandar_datos
                                            .WriteLine(resultExtended); // Mandamos el comando de la variable anterior
                                        this.mandar_datos.Flush(); // ..
                                    }
                                }

                                break;
                            }

                            this.mandar_datos.WriteLine(result); // Mandamos el comando de la variable anterior
                            this.mandar_datos.Flush(); // ..
                            continue;
                        }
                    }
                }
            } //END While Interior
        } //END While Eterno
    }

    private string ProcesarUnionConexionCanal(string lineaCompleta, Match match)
    {
        string usuario = match.Groups[1].Value;
        string canal = match.Groups[4].Value.Trim();

        cDicListaUsuarios_HiperVinculos.TryAdd(usuario, new List<Trio<string, List<string>, DateTime>>());

        Console.WriteLine($"{usuario} ha entrado al canal {canal}");
        return $"PRIVMSG {canal} :Bienvenido {usuario}!";
    }

    private string ProcesarAbandonarConexionCanal(string lineaCompleta, Match match)
    {
        string usuario = match.Groups[1].Value;
        string canal = match.Groups[4].Value.Trim();

        cDicListaUsuarios_HiperVinculos.TryRemove(usuario, out _);

        Console.WriteLine($"{usuario} ha salido del canal {canal} Chao Chao!");
        return $"PRIVMSG {canal} :Bye Bye {usuario}!";
    }

    string Pong(string item, Match regex) //Para responder al Ping del servidor con Pong
    {
        return
            "PONG " + regex.Groups[1].Value; // Capturamos lo que esta despues del ping y le damos al pong con los datos
    }

    string ProcesarMensajeRegular(string item, Match regex)
    {
        this.dedonde = regex.Groups[3].Value; // Se detecta la procedencia del mensaje
        this.usuarioCanal = regex.Groups[1].Value; // Quien manda el mensaje
        this.mensaje = regex.Groups[4].Value; // El mensaje en sí

        #region En otros servers se detecta así

        /*string dedonde = regex.Groups[1].Value; // Se detecta la procedencia del mensaje
        string mensaje = regex.Groups[4].Value; // El mensaje en si*/

        #endregion

        if (this.dedonde ==
            this.canal) // Si la procedencia del mensaje no es el canal en si activamos esta condicion , cabe aclarar que si es el canal
            // el que nos mando el mensaje es un mensaje PUBLICO , caso contrario es PRIVADO

        {
            Console.WriteLine("[+] " + this.dedonde + " dice : " +
                              this.mensaje); // Mostramos el dueño del mensaje y el mensaje
            // Esta es la orden !spam con los (.*) detectamos los dos comandos que son <nick> <mensaje>
            Match regex_ordenes = Regex.Match(this.mensaje, "!spam (.*) (.*)", RegexOptions.IgnoreCase);

            if (regex_ordenes.Success)
            {
                return "";
            }
        }
        //Si la procedencia es un privado
        else if (this.dedonde == this.nickname)
        {
            // Mostramos el dueño del mensaje y el mensaje
            Console.WriteLine("[+] " + this.usuarioCanal + " Como privado a " + this.dedonde + " dice : " +
                              this.mensaje);
            // Esta es la orden !spam con los (.*) detectamos los dos comandos que son <nick> <mensaje> y los ignoramos
            Match regex_ordenes = Regex.Match(this.mensaje, "!spam (.*) (.*)", RegexOptions.IgnoreCase);

            if (regex_ordenes.Success)
            {
                return "";
            }
        }

        return "°°°" + this.mensaje;
    }

    bool Cleaner()
    {
        try
        {
            ThreadCleaner();
            UserHyperLinkCleaner();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Cleaner: " + e.Message);
            return false;
        }
    }

    bool ThreadCleaner()
    {
        try
        {
            if (thrGeminiExplorations != null)
            {
                thrGeminiExplorations.RemoveAll(thr => !thr.IsAlive);
                return true;
            }

            thrGeminiExplorations = new();
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine("ThreadCleaner: " + e.Message);
            return false;
        }
    }

    bool UserHyperLinkCleaner()
    {
        try
        {
            List<string> l_keysToRemove = new List<string>();
            TimeSpan tSpan = new TimeSpan(3, 0, 0);

            if (cDicListaUsuarios_HiperLinkActivo != null)
            {
                foreach (KeyValuePair<string, Par<string, DateTime>> kvp in cDicListaUsuarios_HiperLinkActivo)
                {
                    if (DateTime.Now - kvp.Value.Item2 > tSpan)
                    {
                        l_keysToRemove.Add(kvp.Key);
                    }
                }
            }

            foreach (string strKeyToRemove in l_keysToRemove)
            {
                cDicListaUsuarios_HiperLinkActivo.TryRemove(strKeyToRemove, out _);
                cDicUsuarios_Historial.TryRemove(strKeyToRemove, out _);
            }

            if (cDicListaUsuarios_HiperVinculos != null)
            {
                foreach (KeyValuePair<string, List<Trio<string, List<string>, DateTime>>> vr in
                         cDicListaUsuarios_HiperVinculos)
                {
                    vr.Value.RemoveAll(c => (DateTime.Now - c.Item3) > tSpan);
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("UserHyperLinkCleaner: " + e.Message);
            return false;
        }
    }

    #endregion

    #region Functions that manage other processes proper of IRC

    string ListaUsuarios(string item, Match regex)
    {
        try
        {
            string usuarios_lista = regex.Groups[4].Value; // Tenemos la variable con todos los nicks

            // Para mayor comodidad usamos un split para separar todos los espacios vacios que estan entre
            this.usuarios = usuarios_lista.Split(' ');
            // cada nick del canal para despues hacer una lista , que es la primera que declare en el codigo
            foreach (string usuario in
                     this.usuarios) // Usamos un for each para leer la lista usuarios y mostrar cada nick en la variable usuario
            {
                cDicListaUsuarios_HiperVinculos.TryAdd(usuario.TrimStart('~', '&', '@', '%', '+'),
                    new List<Trio<string, List<string>, DateTime>>());
                Console.WriteLine("[+] User : " + usuario); // Mostramos cada user
            }

            return usuarios_lista;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    string ObtenerListaUsuarios(string item, Match regex)
    {
        try
        {
            string canal = regex.Groups[4].Value.Trim();

            return $"NAMES {canal}";
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return String.Empty;
        }
    }

    #endregion

    #region Functions than make the bot do something (This is where the extended functions should go)

    public string Back(string item, Match regex)
    {
        try
        {
            return string.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine("Back: " + e.Message);
            return string.Empty;
        }
    }

    public string DadosDeRol(string item, Match regex)
    {
        string[] numerosRelevnateDado = item.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0]
            .Split("d", StringSplitOptions.RemoveEmptyEntries);

        if (!int.TryParse(numerosRelevnateDado[0].TrimStart("!"), out _) ||
            !int.TryParse(numerosRelevnateDado[1], out _))
        {
            //Significa que no era un uso de tirada de dado, es una falsa detección
            return string.Empty;
        }

        int a = 0, b = 0, resultInt = 0, c = 0;
        string donde = this.dedonde;
        string resultStr = string.Empty;
        if (int.TryParse(regex.Groups[1].Value, out a) && int.TryParse(regex.Groups[2].Value, out b))
        {
            if (a > 0 && b > 0)
            {
                Random rand = new Random();
                if (donde != this.canal)
                {
                    donde = this.usuarioCanal;
                }

                string quien = !string.IsNullOrWhiteSpace(this.usuarioCanal)
                    ? " " + this.usuarioCanal + " "
                    : " ";

                string strSingularPlural = string.Empty;
                resultStr = "PRIVMSG" + " " + donde + " " + ":Has tirado " + a + " dados de " + b +
                            " caras, los resultados son:|n|";
                resultInt = 0;
                for (int i = 0; i < a; i++)
                {
                    c = rand.Next(1, b);
                    if (a == 1)
                    {
                        resultInt = c;
                        return "PRIVMSG" + " " + donde + " " + ":El usuario" + quien + "ha tirado " + a + " dado de " +
                               b + " caras, el resultado fue: " + resultInt; // Mandamos
                    }

                    resultInt += c;
                    resultStr += "PRIVMSG" + " " + donde + " " + ":En el " + (i + 1) + "° lanzamiento sacaste: " + c +
                                 "|n|";
                }

                resultStr += "PRIVMSG" + " " + donde + " " +
                             ":Todas las tiradas anteriores contabilizan un total de: " + resultInt + "!!!";
                return resultStr;
            }
            else
            {
                return "PRIVMSG" + " " + donde + " " +
                       ":Debes tirar al menos un dado para obtener un resultado! (Sintaxis de ejemplo: !1d4 = 1 dado de 4 caras)"; // Mandamos
            }
        }
        else
        {
            return "PRIVMSG" + " " + donde + " " +
                   ":Debes insertar valores numericos validos y exactos para obtener un resultado! (Sintaxis de ejemplo: !1d4 = 1 dado de 4 caras)"; // Mandamos
        }
    }

    public string FetchGeminiSite(string item, Match regex)
    {
        try
        {
            Thread thrd = new Thread(() => FetchGeminiGem(item, regex));
            Cleaner();
            thrGeminiExplorations.Add(thrd);
            thrd.Start();
            return "1";
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return "0";
        }
    }

    public string FetchGopherSite(string item, Match regex)
    {
        try
        {
            Thread thrd = new Thread(() => FetchGopherHole(item, regex));
            Cleaner();
            thrGeminiExplorations.Add(thrd);
            thrd.Start();
            return "1";
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return "0";
        }
    }

    public async Task<string> FetchGeminiGem(string item, Match regex)
    {
        try
        {
            string donde = this.dedonde;

            if (donde != this.canal)
            {
                donde = this.usuarioCanal;
            }

            string quien = !string.IsNullOrWhiteSpace(this.usuarioCanal) ? this.usuarioCanal : " ";

            DateTime lastExecution = DateTime.Now;
            TimeSpan interval = new TimeSpan(0, 0, 0, 2, 50);
            string abb = FetchGeminiSiteAsync(item, regex).GetAwaiter().GetResult();
            string[] strArray = abb.Split("\n");
            int i = 0, j = 0;
            string result = string.Empty;
            string baseUrl = item;
            if (baseUrl.Contains(">"))
            {
                baseUrl = item.Split(">", StringSplitOptions.TrimEntries)[1];
            }

            cDicListaUsuarios_HiperLinkActivo.AddOrUpdate(quien, new Par<string, DateTime>(baseUrl, DateTime.Now),
                (key, value) => new Par<string, DateTime>(baseUrl, DateTime.Now));

            cDicUsuarios_Historial.AddOrUpdate(
                quien,
                new ConcurrentStack<string>(new[] { baseUrl }),
                (key, existingStack) =>
                {
                    existingStack.Push(baseUrl);
                    return existingStack;
                }
            );

            if (!cDicListaUsuarios_HiperVinculos.ContainsKey(quien))
            {
                cDicListaUsuarios_HiperVinculos.TryAdd(quien, new List<Trio<string, List<string>, DateTime>>());
            }

            Trio<string, List<string>, DateTime> trio = new();
            List<string> l_hiper = new List<string>();
            do
            {
                if (interval <= (DateTime.Now - lastExecution))
                {
                    lastExecution = DateTime.Now;
                    if (strArray[i].Contains("=>"))
                    {
                        if (!strArray[i].Contains("gemini:"))
                        {
                            l_hiper.Add("=> " + baseUrl + strArray[i].Replace("=> ", "").TrimEnd());
                        }
                        else
                        {
                            l_hiper.Add(strArray[i].TrimEnd());
                        }

                        if (strArray[i].Contains('\t'))
                        {
                            WriterSender.TryWrite(new Trio<Bot, string, string>(this, donde,
                                "[" + j + "] " +
                                strArray[i].Replace("\t\t", "\t").Split('\t', StringSplitOptions.TrimEntries)[1]
                                    .TrimEnd()));
                        }
                        else
                        {
                            WriterSender.TryWrite(new Trio<Bot, string, string>(this, donde,
                                "[" + j + "] " + strArray[i].TrimEnd()));
                        }

                        i++;
                        j++;
                        // Console.WriteLine("[" + i + "] " + strArray[i]);
                        continue;
                    }

                    WriterSender.TryWrite(new Trio<Bot, string, string>(this, donde, strArray[i].TrimEnd()));
                    i++;
                }
            } while (strArray.Length > i);

            trio = new Trio<string, List<string>, DateTime>(baseUrl, l_hiper, DateTime.Now);

            List<Trio<string, List<string>, DateTime>> l_baseTrio = new();
            List<Trio<string, List<string>, DateTime>> l_originalTrio = new();
            cDicListaUsuarios_HiperVinculos.TryGetValue(quien, out l_baseTrio);
            l_originalTrio = l_baseTrio;
            l_baseTrio.Add(trio);
            cDicListaUsuarios_HiperVinculos.TryUpdate(quien, l_baseTrio, l_originalTrio);

            result = "Commands available: [B]ack ; [#] to use link ; You are Here: " + baseUrl;
            WriterSender.TryWrite(new Trio<Bot, string, string>(this, donde, result));
            return string.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine("FetchGeminiGem: " + e.Message);
            return string.Empty;
        }
    }

    public async Task<string> FetchGopherHole(string item, Match regex)
    {
        try
        {
            string donde = this.dedonde;

            if (donde != this.canal)
            {
                donde = this.usuarioCanal;
            }

            string quien = !string.IsNullOrWhiteSpace(this.usuarioCanal) ? this.usuarioCanal : " ";

            DateTime lastExecution = DateTime.Now;
            TimeSpan interval = new TimeSpan(0, 0, 0, 2, 50);
            string abb = FetchGopherSiteAsync(item, regex).GetAwaiter().GetResult();
            string[] strArray = abb.Split("\n");
            int i = 0, j = 0;
            string result = string.Empty;
            string baseUrl = item;
            if (baseUrl.Contains(">"))
            {
                baseUrl = item.Split(">", StringSplitOptions.TrimEntries)[1];
            }

            cDicListaUsuarios_HiperLinkActivo.AddOrUpdate(quien, new Par<string, DateTime>(baseUrl, DateTime.Now),
                (key, value) => new Par<string, DateTime>(baseUrl, DateTime.Now));

            cDicUsuarios_Historial.AddOrUpdate(
                quien,
                new ConcurrentStack<string>(new[] { baseUrl }),
                (key, existingStack) =>
                {
                    existingStack.Push(baseUrl);
                    return existingStack;
                }
            );

            if (!cDicListaUsuarios_HiperVinculos.ContainsKey(quien))
            {
                cDicListaUsuarios_HiperVinculos.TryAdd(quien, new List<Trio<string, List<string>, DateTime>>());
            }

            Trio<string, List<string>, DateTime> trio = new();
            List<string> l_hiper = new List<string>();
            do
            {
                if (interval <= (DateTime.Now - lastExecution))
                {
                    lastExecution = DateTime.Now;

                    if (strArray[i].Length < 2)
                    {
                        i++;
                        WriterSender.TryWrite(new Trio<Bot, string, string>(this, donde, " "));
                        continue;
                    }

                    string firstCharacter = strArray[i].Substring(0, 2);
                    if ((firstCharacter.Contains("0")
                         || firstCharacter.Contains("4")
                         || firstCharacter.Contains("5")
                         || firstCharacter.Contains("6")
                         || firstCharacter.Contains("9"))
                        && (!firstCharacter.Contains(".")
                            && !firstCharacter.Contains("-")
                            && !firstCharacter.Contains(")")
                            && !firstCharacter.Contains("]"))
                       )
                    {
                        string getTypeFile = strArray[i].Substring(0, 1);
                        if (!strArray[i].Contains("gopher:"))
                        {
                            string rootUrl = baseUrl.Substring(0, baseUrl.Replace("gopher://", "").IndexOf("/") + 9);
                            l_hiper.Add(rootUrl + "/" + getTypeFile +
                                        strArray[i].Split('\t', StringSplitOptions.TrimEntries)[1].TrimEnd());
                        }
                        else
                        {
                            l_hiper.Add(strArray[i].TrimEnd());
                        }

                        if (strArray[i].Contains('\t'))
                        {
                            WriterSender.TryWrite(new Trio<Bot, string, string>(this, donde,
                                "[" + j + "] " + strArray[i].Split('\t', StringSplitOptions.TrimEntries)[0].TrimEnd()));
                        }
                        else
                        {
                            WriterSender.TryWrite(new Trio<Bot, string, string>(this, donde,
                                "[" + j + "] " + strArray[i].TrimEnd()));
                        }

                        i++;
                        j++;
                        // Console.WriteLine("[" + i + "] " + strArray[i]);
                        continue;
                    }

                    WriterSender.TryWrite(new Trio<Bot, string, string>(this, donde, strArray[i].TrimEnd()));
                    i++;
                }
            } while (strArray.Length > (i + 1));

            trio = new Trio<string, List<string>, DateTime>(baseUrl, l_hiper, DateTime.Now);

            List<Trio<string, List<string>, DateTime>> l_baseTrio = new();
            List<Trio<string, List<string>, DateTime>> l_originalTrio = new();
            cDicListaUsuarios_HiperVinculos.TryGetValue(quien, out l_baseTrio);
            l_originalTrio = l_baseTrio;
            l_baseTrio.Add(trio);
            cDicListaUsuarios_HiperVinculos.TryUpdate(quien, l_baseTrio, l_originalTrio);

            result = "Commands available: [B]ack ; [#] to use link ; You are Here: " + baseUrl;
            WriterSender.TryWrite(new Trio<Bot, string, string>(this, donde, result));
            // return result;
            return string.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine("FetchGopherHole: " + e.Message);
            return string.Empty;
        }
    }

    public static async Task<string> FetchGeminiSiteAsync(string urlString, Match regex)
    {
        try
        {
            Uri uri = null;
            if (urlString.Contains(">"))
            {
                uri = new Uri(urlString.Split(">", StringSplitOptions.TrimEntries)[1]);
            }
            else
            {
                uri = new Uri(urlString);
            }

            if (!uri.Scheme.Equals("gemini", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("URL must use the gemini:// scheme.");

            // Gemini protocol defaults to port 1965
            int port = uri.Port == -1 ? 1965 : uri.Port;

            using TcpClient client = new TcpClient();
            await client.ConnectAsync(uri.Host, port);

            // Establish the TLS stream
            using var sslStream = new SslStream(client.GetStream(), false,
                (sender, certificate, chain, sslPolicyErrors) =>
                    true); // Trust TOFU/Self-signed certs typical of Gemini

            await sslStream.AuthenticateAsClientAsync(uri.Host);

            // Gemini requests are structured exactly as: <URL><CR><LF>
            byte[] requestBytes = Encoding.UTF8.GetBytes($"{uri}\r\n");
            await sslStream.WriteAsync(requestBytes, 0, requestBytes.Length);
            await sslStream.FlushAsync();

            // Read the server response
            using var reader = new StreamReader(sslStream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return string.Empty;
        }
    }

    public static async Task<string> FetchGopherSiteAsync(string urlString, Match regex)
    {
        Uri uri = null;
        if (urlString.Contains(">"))
        {
            uri = new Uri(urlString.Split(">", StringSplitOptions.TrimEntries)[1]);
        }
        else
        {
            uri = new Uri(urlString);
        }

        if (!uri.Scheme.Equals("gopher", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("La URL debe ser absoluta y usar el esquema gopher://");
        }

        // El protocolo Gopher usa el puerto 70 por defecto
        int port = uri.Port == -1 ? 70 : uri.Port;

        using var client = new TcpClient();
        await client.ConnectAsync(uri.Host, port);

        // Gopher es texto plano directo sobre el stream de red (Sin TLS/SSL)
        using var networkStream = client.GetStream();

        // Extraer el selector (Gopher requiere el path limpio sin el caracter de tipo inicial)
        // uri.AbsolutePath usualmente incluye un "/" inicial. Si está vacío o es solo "/", se envía cadena vacía.
        string selector = uri.AbsolutePath;
        string filtrado = selector.Replace("//", "");
        if (selector.Length >= 3)
        {
            filtrado = selector.Substring(3).Replace("//", "");
        }

        // Las solicitudes Gopher se estructuran exactamente como: <Selector><CR><LF>
        byte[] requestBytes = Encoding.UTF8.GetBytes(filtrado.TrimStart('/').TrimEnd('/') + "\r\n");
        await networkStream.WriteAsync(requestBytes);
        await networkStream.FlushAsync();

        // Leer la respuesta completa del servidor
        using var reader = new StreamReader(networkStream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    public string FollowLinkSite(string item, Match regex)
    {
        try
        {
            string str = item.Replace("[", "").Replace("]", "");
            int linkNumero = 0;

            string donde = this.dedonde;

            if (donde != this.canal)
            {
                donde = this.usuarioCanal;
            }

            string quien = !string.IsNullOrWhiteSpace(this.usuarioCanal) ? this.usuarioCanal : " ";

            if (!int.TryParse(str, out linkNumero))
            {
                if ((str == "B") || (str == "b"))
                {
                    ConcurrentStack<string> cQHistory = new ConcurrentStack<string>();
                    cDicUsuarios_Historial.TryGetValue(quien, out cQHistory);
                    string backUrl = string.Empty;
                    int i = 0;
                    bool tryPop = false;
                    if (cQHistory == null || cQHistory.Count <= 1)
                    {
                        WriterSender.TryWrite(new Trio<Bot, string, string>(this, donde,
                            "There is no place before this one to go back to, try another option or command"));
                        return "1";
                    }
                    do
                    {
                        tryPop = cQHistory.TryPop(out backUrl);
                        if (i < 1)
                        {
                            tryPop = false;
                        }

                        i++;
                    } while (!tryPop);

                    if (backUrl.Contains("gemini"))
                    {
                        Thread thrd = new Thread(() => FetchGeminiSite(backUrl, regex));
                        thrGeminiExplorations.Add(thrd);
                        thrd.Start();
                    }
                    else if (backUrl.Contains("gopher"))
                    {
                        Thread thrd = new Thread(() => FetchGopherSite(backUrl, regex));
                        thrGeminiExplorations.Add(thrd);
                        thrd.Start();
                    }
                    else
                    {
                    }

                    Cleaner();
                    Console.WriteLine("FollowLinkSite");
                    return "1";
                }

                WriterSender.TryWrite(new Trio<Bot, string, string>(this, donde,
                    "Favor enviar el número del link requerido"));
                return "0";
            }

            List<Trio<string, List<string>, DateTime>> info = new List<Trio<string, List<string>, DateTime>>();
            cDicListaUsuarios_HiperVinculos.TryGetValue(quien, out info);
            if (info.Count == 0)
            {
                WriterSender.TryWrite(new Trio<Bot, string, string>(this, donde,
                    "Link Inválido, por favor intente con otro"));
                return "0";
            }

            Par<string, DateTime> Par_currentActiveUrl_TimeMark = new();
            cDicListaUsuarios_HiperLinkActivo.TryGetValue(quien, out Par_currentActiveUrl_TimeMark);

            Trio<string, List<string>, DateTime>
                precise = info.Where(c => c.Item1 == Par_currentActiveUrl_TimeMark.Item1).FirstOrDefault();
            if (precise.Item2.Count == 0)
            {
                Console.WriteLine("FollowLinkSite: tiro Default");
                return "0";
            }

            string prepare = string.Empty;

            if (precise.Item1.Contains("gemini"))
            {
                prepare = precise.Item2[linkNumero].Replace("=> ", "").Split("\t", StringSplitOptions.TrimEntries)[0];
                Thread thrd = new Thread(() => FetchGeminiSite(prepare, regex));
                thrGeminiExplorations.Add(thrd);
                thrd.Start();
            }
            else if (precise.Item1.Contains("gopher"))
            {
                prepare = precise.Item2[linkNumero];
                Thread thrd = new Thread(() => FetchGopherSite(prepare, regex));
                thrGeminiExplorations.Add(thrd);
                thrd.Start();
            }
            else
            {
            }

            Cleaner();
            Console.WriteLine("FollowLinkSite");
            return "1";
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return "0";
        }
    }

    #endregion
}