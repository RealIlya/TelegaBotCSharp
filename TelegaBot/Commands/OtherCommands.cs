using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TelegaBot.BotStuff;
using TelegaBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

namespace TelegaBot.Commands
{
    public class OtherCommands
    {
        private readonly Random _random = new Random();

        private readonly List<Student> _students;

        private int _lastSeenDay;

        private List<Student> _todayDuties;


        public OtherCommands()
        {
            _lastSeenDay = DateTime.Today.Day;

            _students = File.Exists("Students.json")
                ? JsonConvert.DeserializeObject<List<Student>>(File.ReadAllText("Students.json"))
                : new List<Student>();
            _todayDuties = new List<Student>()
            {
                _students[_random.Next(0, _students.Count / 2)],
                _students[_random.Next(_students.Count / 2, _students.Count)]
            };
        }

        public async Task<Message> ShowHelp(ITelegramBotClient client, Message message)
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
                ActionType.SendText, AnnouncementType.Regular);
        }

        public async Task<Message> ShowDuty(ITelegramBotClient client, Message message)
        {
            var presentDay = DateTime.Today.Day;
            if (_lastSeenDay != presentDay)
            {
                _lastSeenDay = presentDay;

                _todayDuties = new List<Student>()
                {
                    _students[_random.Next(0, _students.Count / 2)],
                    _students[_random.Next(_students.Count / 2, _students.Count)]
                };
            }

            return await client.ToMessageAsync(message,
                $"Дежурят сегодня - <b>{string.Join(" и ", _todayDuties)}</b>",
                ActionType.SendText, AnnouncementType.Regular);
        }

        public async Task<Message> ShowTime(ITelegramBotClient client, Message message)
        {
            return await client.ToMessageAsync(message,
                $"Дата: <b>{DateTime.Now.ToString($"d-M-yyyy")}</b>\n" +
                $"Время <b>{DateTime.Now.ToString($"H:m:s")}</b>",
                ActionType.SendText, AnnouncementType.Regular);
        }


        public Task<Message> ShowBlackFrame(ITelegramBotClient client, Message message)
        {
            throw new NotImplementedException();
        }

        public async Task<Message> Echo(ITelegramBotClient client, Message message, List<string> textList)
        {
            if (textList.Count < 3)
                return await client.ToMessageAsync(message,
                    "Слишком мало аргументов, образец: /echo <i>продолжительность задержка слово</i>",
                    ActionType.SendText, AnnouncementType.Info);

            if (!int.TryParse(textList.First(), out int checkDuration) || checkDuration > 100)
                return await client.ToMessageAsync(message,
                    "Продолжительность > 100 мс",
                    ActionType.SendText, AnnouncementType.Error);

            if (!int.TryParse(textList[1], out int checkDelay) || checkDelay > 10000)
                return await client.ToMessageAsync(message,
                    "Задержка > 10000 мс",
                    ActionType.SendText, AnnouncementType.Error);

            var duration = int.Parse(textList.Pop(0));
            var delay = int.Parse(textList.Pop(0));

            if (string.Join("", textList) is { Length: > 100 })
                return await client.ToMessageAsync(message,
                    "Слово содержит > 100 букв",
                    ActionType.SendText, AnnouncementType.Error);

            for (int i = 0; i < duration; i++)
            {
                await Task.Delay(delay);
                await client.ToMessageAsync(message, string.Join(" ", textList),
                    ActionType.SendText, AnnouncementType.Regular);
            }

            return null;
        }
    }
}