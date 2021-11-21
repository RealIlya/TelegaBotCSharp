using System.Threading.Tasks;
using TelegaBot.BotStuff;
using TelegaBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegaBot.Commands
{
    public class RunCommand : ICommand
    {
        public Task<Message> Execute(ITelegramBotClient client, Message message)
        {
            throw new System.NotImplementedException();
        }

        public bool CanExecute(Message message, out (string, AnnouncementType) error)
        {
            error = (null, AnnouncementType.Info);
            return true;
        }
    }
}