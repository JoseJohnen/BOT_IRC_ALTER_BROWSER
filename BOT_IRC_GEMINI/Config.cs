using Microsoft.Extensions.Configuration;

namespace BOT_IRC_GEMINI;

public static class Config
{
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