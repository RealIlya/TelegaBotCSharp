using System.Collections.Generic;
using System.Threading.Tasks;
using TelegaBot.BotStuff;
using TelegaBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegaBot.AnswerCallbacks
{
    public class ShowTimetableSelectorAnswer : IAnswer
    {
        public async Task<Message> Execute(ITelegramBotClient client, CallbackQuery callbackQuery) {
            return await client.ToMessageAsync(callbackQuery.Message, "Выберите день недели:",
                client.CheckYourself(callbackQuery.Message.From.Id) ? ActionType.EditText : ActionType.SendText,
                keyboardMarkup: MarkupConstructor.CreateMarkup(3, 2, GlConsts.DayOfWeeks, "timetable",
                    new Dictionary<string, string>() { { "timetable6", "Сегодня" } }));
        }

        public bool CanExecute(CallbackQuery callbackQuery, out (string, AnnouncementType) error)
        {
            error = (null, AnnouncementType.Info);
            return true;
        }
    }
}