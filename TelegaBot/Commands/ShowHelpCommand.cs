using System.Threading.Tasks;
using TelegaBot.BotStuff;
using TelegaBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegaBot.Commands
{
    public class ShowHelpCommand : ICommand
    {
        public async Task<Message> Execute(ITelegramBotClient client, Message message)
        {
            return await client.ToMessageAsync(message,
                "<b>Для просмотра дежурных</b> - /work\n" +
                "<b>Расписание</b> - /timetable\n" +
                "<b>Точное время</b> - /time\n" +
                "<b>Домашнее задание</b> - /hw\n" +
                "<b>Демотиватор</b> - /black <i>картинка</i>\n\n" +
                "<b>Параметры без запятых.</b>\n\n" +
                "<b>Сохранить домашнее задание в бота</b> -\n" +
                "/newhw <i>предмет, дата (или сегодня), день недели (или сегодня), приложить картинку</i>",
                ActionType.SendText);
        }

        public bool CanExecute(Message message, out (string, AnnouncementType) error)
        {
            error = (null, AnnouncementType.Info);
            return true;
        }
    }
}