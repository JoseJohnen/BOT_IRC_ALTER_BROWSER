namespace BOT_IRC_GEMINI;

class Program
{
    public static Bot bot = null;

    private static string servidor = "irc.libera.chat";
    private static string nombreBot = "ClapTrakaLaKa";
    private static string nCanal = "#locos";

    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        MainAsync();
    }

    static void MainAsync()
    {
        bot = CreateBot();

        if (bot != null)
        {
            bot.WorkingBot();
        }
    }

    public static Bot CreateBot()
    {
        try
        {
            Restart:
            Console.Clear();
            Console.WriteLine("A Que servidor desea conectar el BOT?");
            string strServidor = Console.ReadLine();

            if (strServidor.ToUpper().Equals("GO"))
            {
                goto finalizar;
            }

            servidor = strServidor;

            //if Invalid
            if (string.IsNullOrWhiteSpace(servidor))
            {
                Console.WriteLine("Nombre Inválido, intente nuevamente");
                Console.WriteLine("Presione cualquier tecla para continuar");
                Console.ReadLine();
                goto Restart;
            }

            nombreBot:
            Console.Clear();
            Console.WriteLine("Perfecto, entonces se connectará al servidor: " + servidor);
            Console.WriteLine("Como desea que se llame el Bot?");
            nombreBot = Console.ReadLine();

            //if Invalid
            if (string.IsNullOrWhiteSpace(nombreBot))
            {
                Console.WriteLine("Nombre Inválido, intente nuevamente");
                Console.WriteLine("Presione cualquier tecla para continuar");
                Console.ReadLine();
                goto nombreBot;
            }

            nCanalRtr:
            Console.Clear();
            Console.WriteLine("Perfecto, entonces se connectará al servidor: " + servidor);
            Console.WriteLine("y se llamará: " + nombreBot);
            Console.WriteLine("A que canal desea que se Connecte el Bot?");
            nCanal = Console.ReadLine();

            //if Invalid
            if (string.IsNullOrWhiteSpace(nCanal))
            {
                Console.WriteLine("Nombre Inválido, intente nuevamente");
                Console.WriteLine("Presione cualquier tecla para continuar");
                Console.ReadLine();
                goto nCanalRtr;
            }

            Console.Clear();
            Console.WriteLine("¡Buenops! :D");
            Console.WriteLine("Connectando al servidor: " + servidor);
            Console.WriteLine("Con el nombre: " + nombreBot);
            Console.WriteLine("Al Canal: " + nCanal);

            finalizar:
            //Bot("chat.freenode.net", "ClapTrakaLaKa", "#locos");
            return new Bot(servidor, nombreBot, nCanal);
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
            return null;
        }
    }
}