using System;
using System.Threading.Tasks;
using TelegaBot.BotStuff;
using TelegaBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegaBot.Commands
{
    public class ShowTimeCommand : ICommand
    {
        public async Task<Message> Execute(ITelegramBotClient client, Message message)
        {
            return await client.ToMessageAsync(message,
                $"Дата: <b>{DateTime.Now.ToString($"d.M.yyyy")}</b>\n" +
                $"Время <b>{DateTime.Now.ToString($"H:m:s")}</b>",
                ActionType.SendText);
        }

        public bool CanExecute(Message message, out (string, AnnouncementType) error)
        {
            error = (null, AnnouncementType.Info);
            return true;
        }
    }
}