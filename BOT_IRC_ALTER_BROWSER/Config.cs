using Microsoft.Extensions.Configuration;

namespace BOT_IRC_ALTER_BROWSER;

public static class Config
{
    public static bool Config_PrepararHostings(IConfiguration configurationRoot)
    {
        try
        {
            if ((configurationRoot[$"Gemini:StartHosting"].ToUpper() == "ALLOW") ||
                (configurationRoot[$"Gopher:StartHosting"].ToUpper() == "ALLOW"))
            {
                Console.Out.WriteLine("Preparing Authorized Hostings");
                if (configurationRoot[$"Gemini:StartHosting"].ToUpper() == "ALLOW")
                {
                    Gemini._pth = configurationRoot[$"Gemini:RootFolder"];
                    Gemini._prt = configurationRoot["Gemini:Port"];
                    Gemini._cert1 = configurationRoot["Gemini:Cert1"];
                    Gemini._cert2 = configurationRoot["Gemini:Cert2"];
                    
                    Program._GeminiServer = new Thread(() => Gemini.StartGeminiServer());
                    Program._GeminiServer.Start();

                    
                }

                if (configurationRoot[$"Gopher:StartHosting"].ToUpper() == "ALLOW")
                {
                    Gopher._pth = configurationRoot["Gopher:RootFolder"];
                    Gopher._prt = configurationRoot["Gopher:Port"];
                    Gopher._lch = configurationRoot["Gopher:URL"];
                    
                    Program._GopherServer = new Thread(() => Gopher.StartGopherServer());
                    Program._GopherServer.Start();
                }

                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine("Config_PrepararHostings(): "+e.Message);
            return false;
        }
    }
    
    public static List<Bot> Config_ObtenerBots(IConfiguration configurationRoot)
    {
        Console.Out.WriteLine("Starting To Obtain Bot Values");

        Bot nBot = null;
        List<Bot> l_bots = new List<Bot>();
        int index = 0;
        Bot._thrSendDataIrc.Start();
        var botsSection = configurationRoot.GetSection("Bots");
        if (!botsSection.GetChildren().Any())
        {
            Console.Out.WriteLine("No hay bots definidos en la configuración. Cancelando operación.");
            return new List<Bot>(); // Retorna una lista vacía de inmediato sin iniciar nada
        }
        bool isAllowed = configurationRoot[$"Bots:StartBots"].ToUpper() == "ALLOW" ? true : false;
        while (isAllowed)
        {
            string nickname = configurationRoot[$"Bots:List:{index}:nickname"];
            string password = configurationRoot[$"Bots:List:{index}:password"];

            if (string.IsNullOrWhiteSpace(nickname))
            {
                break;
            }

            string host = configurationRoot[$"Bots:List:{index}:host"];
            string canal = configurationRoot[$"Bots:List:{index}:canal"];

            nBot = new Bot();
            nBot.nickname = nickname;
            nBot.Password = password;
            nBot.host = host;
            nBot.canal = canal;
            Console.Out.WriteLine($"Bot encontrado: {nBot.nickname}");
            l_bots.Add(nBot);
            index++;
        }

        return l_bots;
    }
}