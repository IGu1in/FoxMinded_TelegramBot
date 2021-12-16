using System;

namespace TGBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new UAHBot();

            bot.BotAsync();
            Console.ReadLine();
        }
    }
}
