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
        while (true)
        {
            string nickname = configurationRoot[$"Bots:{index}:nickname"];

            if (string.IsNullOrWhiteSpace(nickname))
            {
                break;
            }

            string host = configurationRoot[$"Bots:{index}:host"];
            string canal = configurationRoot[$"Bots:{index}:canal"];

            nBot = new Bot();
            nBot.nickname = nickname;
            nBot.host = host;
            nBot.canal = canal;
            Console.Out.WriteLine($"Bot encontrado: {nBot.nickname}");
            l_bots.Add(nBot);
            index++;
        }

        return l_bots;
    }
}