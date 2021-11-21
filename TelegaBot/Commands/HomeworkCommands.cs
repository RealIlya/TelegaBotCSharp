using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class HomeworkCommands : HwData
    {
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

        #region Commands block

        public static async Task<Message> ShowSubjectSelector(ITelegramBotClient client, Message message)
        {
            return _homeworks.Count > 0
                ? await client.ToMessageAsync(message, "Выберите предмет:",
                    client.CheckYourself(message.From.Id) ? ActionType.EditText : ActionType.SendText,
                    keyboardMarkup: MarkupConstructor.CreateMarkup(4, 3, GlConsts.Subjects, "subject"))
                : await client.ToMessageAsync(message, _haveNotHomework,
                    client.CheckYourself(message.From.Id) ? ActionType.EditText : ActionType.SendText);
        }

        public static async Task<Message> ShowDayOfWeekSelector(ITelegramBotClient client, CallbackQuery callbackQuery)
        {
            _notCheckedHomework = new Homework();

            var message = callbackQuery.Message;
            var subjectNumber = int.Parse(callbackQuery.Data.Substring("subject".Length));
            var subject = GlConsts.Subjects[subjectNumber];
            
            if (_homeworks.Any(homework => homework.Subject == subject))
            {
                _notCheckedHomework.Subject = subject;
                return await client.ToMessageAsync(message, "Выберите день недели:",
                    client.CheckYourself(message.From.Id) ? ActionType.EditText : ActionType.SendText,
                    keyboardMarkup: MarkupConstructor.CreateMarkup(3, 2, GlConsts.DayOfWeeks, "dayOfWeek",
                        new() { { "dayOfWeekBack", "К списку предметов" } }));
            }

            return await client.ToMessageAsync(message, "Ничего",
                client.CheckYourself(message.From.Id) ? ActionType.EditText : ActionType.SendText,
                keyboardMarkup: MarkupConstructor.CreateMarkup(
                    new() { { "dayOfWeekBack", "К списку предметов" } }));
        }

        public static async Task<Message> ShowHomework(ITelegramBotClient client, CallbackQuery callbackQuery)
        {
            var message = callbackQuery.Message;
            var dayOfWeekNumber = int.Parse(callbackQuery.Data.Substring("dayOfWeek".Length));
            var dayOfWeek = GlConsts.DayOfWeeks[dayOfWeekNumber];

            if (_homeworks.Any(homework => homework.DayOfWeek == dayOfWeek))
            {
                _notCheckedHomework.DayOfWeek = dayOfWeek;
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
                        return await client.ToMessageAsync(message,
                            $"Были найдены домашние задания по предмету {homework.Subject} за {homework.DayOfWeek}.\n" +
                            "Выберите число:",
                            client.CheckYourself(message.From.Id) && message.ReplyToMessage == null
                                ? ActionType.EditText
                                : ActionType.SendText, replyToMessageId: homework.MessageId,
                            keyboardMarkup: MarkupConstructor.CreateMarkup(intersections.Count / 2, 2,
                                intersections.Select(intersection => intersection.Date).ToList(), "intersection",
                                new() { { "homeworkBack", "К началу" } }));
                    }

                    if (homework.ChatId != message.Chat.Id)
                    {
                        return await client.ForwardMessageAsync(message.Chat.Id, homework.ChatId,
                            homework.MessageId);
                    }

                    return await client.ToMessageAsync(message,
                        $"Домашнее задание по {homework.Subject.Substring(0, homework.Subject.Length - 1) + "е"} за {homework.DayOfWeek}",
                        client.CheckYourself(message.From.Id) && message.ReplyToMessage == null &&
                        homework.MessageId == null
                            ? ActionType.EditText
                            : ActionType.SendText,
                        client.CheckYourself(message.From.Id) && message.ReplyToMessage == null &&
                        homework.MessageId == null,
                        replyToMessageId: homework.MessageId,
                        keyboardMarkup: MarkupConstructor.CreateMarkup(
                            new() { { "homeworkBack", "К началу" } }));
                }
            }

            return await client.ToMessageAsync(message, "Ничего", ActionType.EditText,
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
            if (message.Photo == null && message.Document == null)
                return await client.ToMessageAsync(message,
                    GlConsts.HAVE_NOT_A_PHOTO,
                    ActionType.SendText, AnnouncementType.Error);

            if (textList.Count < 2)
                return await client.ToMessageAsync(message,
                    GlConsts.NOT_ENOUGH_ARGUMENTS +
                    "/newhw <i>предмет, дата (или сегодня), день недели (или сегодня), приложить картинку</i>",
                    ActionType.SendText, AnnouncementType.Error);

            var subject = textList.First().ToTitle();
            var dayOfWeek = textList.Middle() == "н" ? GlConsts.DayOfWeeks[Utilities.GetDayOfWeek() - 1] : textList[1].ToTitle();
            var date = textList.Last() == "н" ? DateTime.Now.ToString($"d.M.yyyy") : textList.Last();

            var errorMessage = (GlConsts.Subjects.Contains(subject),
                    GlConsts.DayOfWeeks.Contains(dayOfWeek),
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

            return await client.SendPhotoAsync(message.Chat.Id, message.Photo.Middle().FileId,
                "Картинка - <i>приложена</i> \n" +
                "Предмет - " + subject + "\n" +
                "Дата - " + date + "\n" +
                "День недели - " + dayOfWeek + "\n" +
                "Правильно?",
                replyMarkup: MarkupConstructor.CreateMarkup(1, 2, new List<string> { "Да", "Нет" }, "choice"),
                parseMode: ParseMode.Html);
        }

        public static async Task<Message> SaveHomework(ITelegramBotClient client, Message message)
        {
            _homeworks.Add(_notCheckedHomework);

            return await client.ToMessageAsync(message,
                "Сохранено",
                ActionType.SendText, AnnouncementType.Info,
                deletePreviousMessage: true);
        }

        #endregion
    }
}