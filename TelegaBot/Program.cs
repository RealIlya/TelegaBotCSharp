using System;
using System.Threading;
using TelegaBot.BotStuff;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;

namespace TelegaBot
{
    internal class Program
    {
        private static TelegramBotClient _client;

        private static void Start()
        {
            _client = new TelegramBotClient("2025956416:AAEcQdq_n-9hQBM3aNUCLDQYEU5JSBkeFLg");
            Handlers._commands = new Commands(_client);
            var me = _client.GetMeAsync().Result;
            Console.Title = me.Username;

            using var cts = new CancellationTokenSource();

            _client.StartReceiving(new DefaultUpdateHandler(Handlers.HandleUpdateAsync, Handlers.HandleErrorAsync),
                cancellationToken: cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();
        }

        public static void Main(string[] args)
        {
            Start();
        }
    }
}