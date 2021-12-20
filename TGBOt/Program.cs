using System;
using System.Threading.Tasks;

namespace TGBot
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var bot = new UAHBot();
            await bot.BotAsync();
            Console.ReadLine();
        }
    }
}
