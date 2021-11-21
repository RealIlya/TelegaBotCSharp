using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using TelegaBot.BotStuff;
using TelegaBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegaBot.Commands
{
    public class EchoCommand : ICommand
    {
        public async Task<Message> Execute(ITelegramBotClient client, Message message)
        {
            var wordsWithoutCommand = message.Text?.RemoveACommand(' ');

            var duration = int.Parse(wordsWithoutCommand.Pop(0));
            var delay = int.Parse(wordsWithoutCommand.Pop(0));

            for (int i = 0; i < duration; i++)
            {
                await Task.Delay(delay);
                await client.ToMessageAsync(message, string.Join(" ", wordsWithoutCommand ?? new List<string>()),
                    ActionType.SendText);
            }

            return null;
        }

        public bool CanExecute(Message message, out (string, AnnouncementType) error)
        {
            var wordsWithoutCommand = message.Text?.RemoveACommand(' ');

            if (wordsWithoutCommand is { Count: < 3 })
            {
                error = (GlConsts.NOT_ENOUGH_ARGUMENTS + "/echo <i>продолжительность задержка слово</i>",
                    AnnouncementType.Info);
                return false;
            }

            error = (
                    (int.TryParse(wordsWithoutCommand.First(), out int checkDuration) && checkDuration <= 100),
                    (int.TryParse(wordsWithoutCommand[1], out int checkDelay) && checkDelay <= 10000),
                    (string.Join("", wordsWithoutCommand) is { Length: <= 100 })) switch
                {
                    (false, _, _) => ("Продолжительность > 100 мс", AnnouncementType.Error),
                    (_, false, _) => ("Задержка > 10000 мс", AnnouncementType.Error),
                    (_, _, false) => ("Слово содержит > 100 букв", AnnouncementType.Error),
                    _ => (null, AnnouncementType.Info)
                };

            return error == (null, AnnouncementType.Info);
        }
    }
}