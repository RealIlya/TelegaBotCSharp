using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
    public class HomeworkCommands : GlobalConstants
    {
        private const string _haveNotHomework = "Домашнего задания ещё не добавлено!";
        private static readonly ObservableCollection<Homework> _homeworks;

        private static Homework _notCheckedHomework;

        static HomeworkCommands()
        {
            _homeworks = File.Exists("Homework.json")
                ? JsonConvert.DeserializeObject<ObservableCollection<Homework>>(File.ReadAllText("Homework.json"))
                : new ObservableCollection<Homework>();

            _homeworks.CollectionChanged += (_, _) =>
            {
                Console.WriteLine("Изменено!");
                File.WriteAllText("Homework.json", JsonConvert.SerializeObject(_homeworks));
            };
        }

        #region Commands

        public static async Task<Message> ShowSubjectSelector(ITelegramBotClient client, Message message,
            ActionType actionType, bool deletePreviousMessage = false)
        {
            return _homeworks.Count > 0
                ? await client.ToMessageAsync(message, "Выберите предмет:",
                    actionType, deletePreviousMessage,
                    keyboardMarkup: MarkupConstructor.CreateMarkup(4, 3, Subjects, "subject"))
                : await client.ToMessageAsync(message,
                    _haveNotHomework,
                    actionType, deletePreviousMessage);
        }

        public static async Task<Message> ShowDayOfWeekSelector(ITelegramBotClient client, CallbackQuery callbackQuery,
            ActionType actionType)
        {
            _notCheckedHomework = new Homework();

            var subjectNumber = int.Parse(callbackQuery.Data.Substring("subject".Length));

            if (_homeworks.Any(homework => homework.Subject == Subjects[subjectNumber]))
            {
                _notCheckedHomework.Subject = Subjects[subjectNumber];
                return await client.ToMessageAsync(callbackQuery.Message, "Выберите день недели:",
                    actionType,
                    keyboardMarkup: MarkupConstructor.CreateMarkup(3, 2, DayOfWeeks, "dayOfWeek",
                        new() { { "dayOfWeekBack", "К списку предметов" } }));
            }

            return await client.ToMessageAsync(callbackQuery.Message, "Ничего", actionType,
                keyboardMarkup: MarkupConstructor.CreateMarkup(
                    new() { { "dayOfWeekBack", "К списку предметов" } }));
        }

        public static async Task<Message> ShowHomework(ITelegramBotClient client, CallbackQuery callbackQuery,
            ActionType actionType, bool deletePreviousMessage = false)
        {
            var dayOfWeekNumber = int.Parse(callbackQuery.Data.Substring("dayOfWeek".Length));

            if (_homeworks.Any(homework => homework.DayOfWeek == DayOfWeeks[dayOfWeekNumber]))
            {
                _notCheckedHomework.DayOfWeek = DayOfWeeks[dayOfWeekNumber];
            }

            foreach (var homework in _homeworks)
            {
                if (homework.Subject == _notCheckedHomework.Subject &&
                    homework.DayOfWeek == _notCheckedHomework.DayOfWeek)
                {
                    var intersections = _homeworks.Where(subHomework =>
                            subHomework.Subject == homework.Subject && subHomework.DayOfWeek == homework.DayOfWeek)
                        .ToList();
                    Console.WriteLine("Intersections are " + intersections.Count);
                    if (intersections.Count > 1)
                    {
                        return await client.ToMessageAsync(callbackQuery.Message,
                            $"Были найдены домашние задания по предмету {homework.Subject} за {homework.DayOfWeek}.\n" +
                            "Выберите число:",
                            actionType, homework.MessageId, deletePreviousMessage, 
                            keyboardMarkup: MarkupConstructor.CreateMarkup(intersections.Count / 2, 2,
                                intersections.Select(intersection => intersection.Date).ToList(), "intersection",
                                new() { { "homeworkBack", "К началу" } }));
                    }

                    if (homework.ChatId != callbackQuery.Message.Chat.Id)
                    {
                        return await client.ForwardMessageAsync(callbackQuery.Message.Chat.Id, homework.ChatId,
                            homework.MessageId);
                    }

                    return await client.ToMessageAsync(callbackQuery.Message,
                        $"Домашнее задание по {homework.Subject.Substring(0, homework.Subject.Length - 1) + "е"} за {homework.DayOfWeek}",
                        actionType, replyToMessageId: homework.MessageId, deletePreviousMessage, 
                        keyboardMarkup: MarkupConstructor.CreateMarkup(
                            new() { { "homeworkBack", "К началу" } }));
                }
            }

            return await client.ToMessageAsync(callbackQuery.Message, "Ничего", ActionType.EditText,
                keyboardMarkup: MarkupConstructor.CreateMarkup(new() { { "homeworkBack", "К началу" } }));
        }

        public static async Task<Message> ShowAllHomework(ITelegramBotClient client, Message message)
        {
            return await client.ToMessageAsync(message, 
                _homeworks.Count > 0 ? string.Join("\n", _homeworks) : _haveNotHomework,
                ActionType.SendText);
        }

        public static async Task<Message> NewHomework(ITelegramBotClient client, Message message, List<string> textList)
        {
            if (message.Photo == null)
                return await client.ToMessageAsync(message,
                    "Картинка не приложенп",
                    ActionType.SendText, AnnouncementType.Error);

            if (textList.Count < 2)
                return await client.ToMessageAsync(message,
                    "Слишком мало аргументов, образец:\n" +
                    "/newhw <i>предмет, дата (или сегодня), день недели (или сегодня), приложить картинку</i>",
                    ActionType.SendText, AnnouncementType.Error);

            // var subject = textList.First().ToTitle();
            // if (!Subjects.Contains(subject))
            // {
            //     return await client.ToMessageAsync(message,
            //         "Предмет указан неправильно",
            //         ActionType.SendText, AnnouncementType.Error);
            // }
            //
            // string dayOfWeek = textList.Middle();
            // if (dayOfWeek != "н")
            // {
            //     if (!DayOfWeeks.Contains(dayOfWeek.ToTitle()))
            //     {
            //         return await client.ToMessageAsync(message,
            //             "День недели указан неправильно",
            //             ActionType.SendText, AnnouncementType.Error);
            //     }
            // }
            // else dayOfWeek = DayOfWeeks[Utilities.GetDayOfWeek() - 1];
            //
            // var date = textList.Last();
            // if (date != "н")
            // {
            //     if (!DateTime.TryParse(date, out DateTime checkDate))
            //     {
            //         return await client.ToMessageAsync(message,
            //             "Дата указана неправильно",
            //             ActionType.SendText, AnnouncementType.Error);
            //     }
            // }
            // else date = DateTime.Now.ToString($"d.M.yyyy");

            var subject = textList.First().ToTitle();
            var dayOfWeek = textList.Middle() == "н" ? DayOfWeeks[Utilities.GetDayOfWeek() - 1] : textList[1].ToTitle();
            var date = textList.Last() == "н" ? DateTime.Now.ToString($"d.M.yyyy") : textList.Last();

            var errorMessage = (Subjects.Contains(subject),
                    DayOfWeeks.Contains(dayOfWeek),
                    DateTime.TryParse(date, out DateTime _)) switch
                {
                    (false, _, _) => client.ToMessageAsync(message,
                        "Предмет указан неправильно",
                        ActionType.SendText, AnnouncementType.Error),
                    (_, false, _) => client.ToMessageAsync(message,
                        "День недели указан неправильно",
                        ActionType.SendText, AnnouncementType.Error),
                    (_, _, false) => client.ToMessageAsync(message,
                        "Дата указана неправильно",
                        ActionType.SendText, AnnouncementType.Error),
                    _ => null
                };

            if (errorMessage != null) 
                return await errorMessage;

            _notCheckedHomework = new Homework(subject, dayOfWeek, date, message.MessageId, message.Chat.Id);

            return await client.SendPhotoAsync(message.Chat.Id, message.Photo.First().FileId,
                "Картинка - <i>приложена</i> \n" +
                "Предмет - " + subject + "\n" +
                "Дата - " + date + "\n" +
                "День недели - " + dayOfWeek + "\n" +
                "Правильно?",
                replyMarkup: MarkupConstructor.CreateMarkup(1, 2, new List<string> { "Да", "Нет" }, "choice"),
                parseMode: ParseMode.Html);
        }

        public static async Task<Message> SaveHomework(ITelegramBotClient client, CallbackQuery callbackQuery)
        {
            _homeworks.Add(_notCheckedHomework);

            return await client.ToMessageAsync(callbackQuery.Message,
                "Сохранено",
                ActionType.SendText, AnnouncementType.Info,
                deletePreviousMessage: true);
        }

        #endregion
    }
}