using System;

namespace TGBot
{
    class Program
    {
        static void Main(string[] args)
        {
            StartBotAsync();
            Console.ReadLine();
        }

        static async void StartBotAsync()
        {
            var bot = new UAHBot();
            await bot.BotAsync();
        }
    }
}
