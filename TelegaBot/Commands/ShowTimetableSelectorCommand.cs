using System.Collections.Generic;
using System.Threading.Tasks;
using TelegaBot.BotStuff;
using TelegaBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegaBot.Commands
{
    public class ShowTimetableSelectorCommand : ICommand
    {
        public async Task<Message> Execute(ITelegramBotClient client, Message message)
        {
            return await client.ToMessageAsync(message, "Выберите день недели:",
                client.CheckYourself(message.From.Id) ? ActionType.EditText : ActionType.SendText,
                keyboardMarkup: MarkupConstructor.CreateMarkup(3, 2, GlConsts.DayOfWeeks, "timetable",
                    new Dictionary<string, string>() { { "timetable6", "Сегодня" } }));
        }

        public bool CanExecute(Message message, out (string, AnnouncementType) error)
        {
            error = (null, AnnouncementType.Info);
            return true;
        }
    }
}