using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TelegaBot.Models;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;


namespace TelegaBot.BotStuff
{
    public class Commands : ICommands
    {
        private readonly Random _random = new Random();
        private readonly TelegramBotClient _client;
        private readonly List<KbMarkup> _navigation;

        private readonly List<Student> _students;
        private readonly Dictionary<string, JToken> _timetable;

        private int _lastSeenDay;
        private Student _firstStudent;
        private Student _secondStudent;


        public Commands(TelegramBotClient client)
        {
            _client = client;
            KbMarkup.Client = client;
            _navigation = new List<KbMarkup>();

            _lastSeenDay = DateTime.Today.Day;

            _students = File.Exists("Students.json")
                ? JsonConvert.DeserializeObject<List<Student>>(File.ReadAllText("Students.json"))
                : new List<Student>();
            _firstStudent = _students[_random.Next(0, _students.Count)];
            _secondStudent = _students[_random.Next(0, _students.Count)];

            _timetable = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(
                Timetable.Get("https://lyceum.nstu.ru/rasp/nika_data_16102021_161642.js"));
        }

        public async Task<Message> ShowHelp(Message message)
        {
            return await _client.SendTextMessageAsync(message.Chat.Id,
                "<b>Для просмотра дежурных</b> /work\n" +
                "<b>Расписание</b> - /timetable\n" +
                "<b>Точное время</b> - /time\n" +
                "<b>Домашнее задание</b> - /hw\n" +
                "<b>Демотиватор</b> - /black <i><u>картинка</u></i>\n\n" +
                "<b>Сохранить домашнее задание в бота</b>-\n" +
                "/newhw <i><u>день недели</u> <u>предмет</u> <u>картинка</u></i>",
                ParseMode.Html);
        }

        public async Task<Message> ShowDuty(Message message)
        {
            int presentDay = DateTime.Today.Day;
            if (_lastSeenDay != presentDay)
            {
                _lastSeenDay = presentDay;

                while (_firstStudent == _secondStudent)
                {
                    _firstStudent = _students[_random.Next(0, _students.Count)];
                    _secondStudent = _students[_random.Next(0, _students.Count)];
                }
            }

            return await _client.SendTextMessageAsync(message.Chat.Id,
                $"Дежурят сегодня - <b>{_firstStudent}</b> и <b>{_secondStudent}</b>",
                ParseMode.Html);
        }

        public async Task<Message> ShowTime(Message message)
        {
            return await _client.SendTextMessageAsync(message.Chat.Id,
                $"Дата: <b>{DateTime.Now.ToString($"M-d-yyyy")}</b>\n" +
                $"Время <b>{DateTime.Now.ToString($"H:m:s")}</b>",
                ParseMode.Html);
        }

        public async Task<Message> ShowTimetableSelector(Message message)
        {
            if (_navigation.Count > 1)
            {
                // foreach (KbMarkup kbMarkup in _navigation)
                // {
                //     await kbMarkup.RemoveKeyboardMarkup(message);
                // }
                await _navigation.Last().RemoveKeyboardMarkup(message);

                _navigation.Clear();
            }

            var keyboard = new List<List<InlineKeyboardButton>>()
                { new(), new(), new(), new() { InlineKeyboardButton.WithCallbackData("Закрыть", "b") } };
            int k = 0;

            for (int i = 0; i < 3; i++, k += 2)
            for (int j = 0; j < 2; j++)
                keyboard[i].Add(InlineKeyboardButton.WithCallbackData(_timetable["DAY_NAMES"][k + j].ToString(),
                    $"t{k + j}"));

            _navigation.Add(new KbMarkup("Выберите день недели:", new InlineKeyboardMarkup(keyboard)));
            return await _navigation.First().SendKeyboardMarkup(message);
        }

        public async Task ShowTimetable(CallbackQuery callbackQuery)
        {
            const string path = "48";
            bool flag = true;
            int lesson = 1;
            // int presentWeekday = (int)DateTime.Today.DayOfWeek - 1 == -1 ? 1 : (int)DateTime.Today.DayOfWeek;
            var callbackQueryDataInt = int.Parse(callbackQuery.Data.Substring(1));

            string result = $"<b><u>{_timetable["DAY_NAMES"][callbackQueryDataInt]}</u></b>\n";

            foreach (JToken item in _timetable["CLASS_SCHEDULE"][path]["029"])
            {
                string key = ((JProperty)item).Name;
                if (key.FirstOrDefault().ToString() != (callbackQueryDataInt + 1).ToString()) continue;
                if (key.LastOrDefault() == '3' && flag) lesson += 2;

                var subjects = (from JProperty subject in _timetable["SUBJECTS"]
                    from ssg in _timetable["CLASS_SCHEDULE"][path]["029"][key]["s"]
                    where subject.Name == ssg.ToString()
                    select _timetable["SUBJECTS"][subject.Name].ToString().ToTitle()).ToList();

                var teachers = (from JProperty teacher in _timetable["TEACHERS"]
                    from tsg in _timetable["CLASS_SCHEDULE"][path]["029"][key]["t"]
                    where teacher.Name == tsg.ToString()
                    select _timetable["TEACHERS"][teacher.Name].ToString()).ToList();

                var rooms = (from JProperty room in _timetable["ROOMS"]
                    from rsg in _timetable["CLASS_SCHEDULE"][path]["029"][key]["r"]
                    where room.Name == rsg.ToString()
                    select _timetable["ROOMS"][room.Name].ToString()).ToList();

                if ((lesson - 1) % 2 == 0) result += $"<b>Пара {lesson / 2 + 1}</b>\n";

                result +=
                    $"{lesson++}. {string.Join(" / ", subjects)}({string.Join(" / ", rooms)})\n" +
                    $"    {string.Join(" / ", teachers)}\n";
                flag = false;
            }

            _navigation.Add(new KbMarkup(result,
                new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Назад", "b"))));
            await _navigation.Last().EditKeyboardMarkup(callbackQuery);
        }

        public Task<Message> ShowBlackFrame(Message message)
        {
            throw new NotImplementedException();
        }

        public Task<Message> ShowHomeWork(Message message)
        {
            throw new NotImplementedException();
        }

        public async Task<Message> NewHomeWork(Message message)
        {
            // var day = piecesOfMessage[1];
            // var subject = piecesOfMessage[2];
            // var picture = piecesOfMessage[3];
            // await Client.SendTextMessageAsync(message.Chat.Id, "День - " + day + "\n" +
            //                                                "Предмет - " + subject + "\n" +
            //                                                "Картинка - " + picture + ".\n" +
            //                                                "Правильно?");
            return new Message();
        }

        public async Task<Message> Echo(Message message, List<string> textList)
        {
            if (textList.Count < 3 || int.Parse(textList.First()) > 100 || int.Parse(textList[1]) > 10000)
            {
                return await _client.SendTextMessageAsync(message.Chat.Id, "Некорректный ввод");
            }

            int duration = textList.First() != null ? int.Parse(textList.Pop(0)) : 0;
            int delay = textList.First() != null ? int.Parse(textList.Pop(0)) : 0;

            for (int i = 0; i < duration; i++)
            {
                await Task.Delay(delay);
                await _client.SendTextMessageAsync(message.Chat.Id, string.Join(" ", textList));
            }

            return null!;
        }

        public async Task<Message> ByDefault(Message message)
        {
            return await _client.SendTextMessageAsync(message.Chat.Id, "Абоба");
        }

        public async Task MoveBack(CallbackQuery callbackQuery)
        {
            if (_navigation.Count > 1)
            {
                _navigation.Remove(_navigation.Last());
                await _navigation.Last().EditKeyboardMarkup(callbackQuery);
            }
            else
            {
                await _navigation.Last().RemoveKeyboardMarkup(callbackQuery.Message);
            }
        }
    }
}