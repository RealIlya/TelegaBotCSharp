using System.Threading.Tasks;
using TelegaBot.BotStuff;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegaBot.Interfaces
{
    public interface ICommand
    {
        Task<Message> Execute(ITelegramBotClient client, Message message);
        bool CanExecute(Message message, out (string, AnnouncementType) error);
    }
}