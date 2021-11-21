using System.Threading.Tasks;
using TelegaBot.BotStuff;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegaBot.Interfaces
{
    public interface IAnswer
    {
        Task<Message> Execute(ITelegramBotClient client, CallbackQuery callbackQuery);
        bool CanExecute(CallbackQuery callbackQuery, out (string, AnnouncementType) error);
    }
}