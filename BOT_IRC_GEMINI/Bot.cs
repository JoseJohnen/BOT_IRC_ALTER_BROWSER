using System.Net.Sockets; // Lo mismo que la anterior
using System.Text.RegularExpressions; // No es vital pero me encantan el uso de las expresiones regulares


namespace BOT_IRC_GEMINI;

public class Bot
{
    #region Attributes

    #region Instance Attributes

    public string host = "irc.libera.chat"; // Establecemos la variable string host para tener el host del canal IRC
    public string nickname = "ClapTrakaLaKa"; // Establecemos la variable nickname con el nick del bot
    public string canal = "#locos"; // Establecemos la variable canal con el nombre del canal

    private StreamReader leer_datos; // Establecemos la variable leer_datos como StreamReader
    private StreamWriter mandar_datos; // Establecemos la variable mandar_datos como SteamWriter

    private string[] usuarios; // Creamos el string[] usuarios para tener todos los nicks que estan en el canal
    private NetworkStream conexion; // Establecemos la variable conexion como NetworkStream
    private TcpClient irc; // Establecemos la variable irc como TcpClient
    private string code = ""; // Creamos la variable string que vamos a usar para leer los sockets
    private string dedonde;
    private string usuarioCanal;
    private string mensaje;

    #endregion

    #region static attributes

    //Do not touch, this are the basic functions the bot performs, it filters out 
    //any normal comms from actual manual commands
    private Dictionary<string, Func<string, Match, string>> dicBotBasicFuncitions =
        new Dictionary<string, Func<string, Match, string>>();
        

    //Here you add the commands you want the bot to perform
    private Dictionary<string, Func<string, Match, string>> dicBotExtendedFunctions =
        new Dictionary<string, Func<string, Match, string>>();
        

    #endregion

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
            { ":(.*) 353 (.*) = (.*) :(.*)", ListaUsuarios },
            { ":(.*)!(.*) PRIVMSG (.*) :(.*)", ProcesarMensajeRegular },
        };

        //Here you add the commands you want the bot to perform
        dicBotExtendedFunctions = new Dictionary<string, Func<string, Match, string>>()
        {
            { "!(.*)d(.*)", DadosDeRol },
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

    #endregion

    #region Functions that manage other processes proper of IRC

    string ListaUsuarios(string item, Match regex)
    {
        string usuarios_lista = regex.Groups[4].Value; // Tenemos la variable con todos los nicks

        // Para mayor comodidad usamos un split para separar todos los espacios vacios que estan entre
        this.usuarios = usuarios_lista.Split(' ');
        // cada nick del canal para despues hacer una lista , que es la primera que declare en el codigo
        foreach (string usuario in
                 this.usuarios) // Usamos un for each para leer la lista usuarios y mostrar cada nick en la variable usuario
        {
            Console.WriteLine("[+] User : " + usuario); // Mostramos cada user
        }

        return usuarios_lista;
    }

    #endregion

    #region Functions than make the bot do something (This is where the extended functions should go)

    public string DadosDeRol(string item, Match regex_ordenes)
    {
        int a = 0, b = 0, resultInt = 0, c = 0;
        string donde = this.dedonde;
        string resultStr = string.Empty;
        if (int.TryParse(regex_ordenes.Groups[1].Value, out a) && int.TryParse(regex_ordenes.Groups[2].Value, out b))
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

    #endregion
}