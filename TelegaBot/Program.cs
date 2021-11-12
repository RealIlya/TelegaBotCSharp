using System;
using System.Threading;
using System.Threading.Tasks;
using TelegaBot.BotStuff;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;

namespace TelegaBot
{
    internal class Program
    {
        private static TelegramBotClient _client;

        public async static Task Main(string[] args)
        {
            _client = new TelegramBotClient("Token");
            var me = await _client.GetMeAsync();
            Console.Title = me.Username;

            using var cts = new CancellationTokenSource();

            _client.StartReceiving(new DefaultUpdateHandler(Handlers.HandleUpdateAsync, Handlers.HandleErrorAsync),
                cancellationToken: cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();
        }
    }
}