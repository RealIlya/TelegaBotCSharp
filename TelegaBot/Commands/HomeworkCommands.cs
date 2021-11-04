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
    public class HomeworkCommands : GlobalConstants
    {
        private readonly ObservableCollection<Homework> _homeworks;

        private Homework _notCheckedHomework;

        public HomeworkCommands()
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

        public async Task<Message> ShowSubjectSelector(ITelegramBotClient client, Message message,
            ActionType actionType, bool deletePreviousMessage = false)
        {
            return await client.ToMessageAsync(message, "Выберите предмет:",
                actionType, AnnouncementType.Regular, deletePreviousMessage,
                keyboardMarkup: MarkupConstructor.CreateMarkup(4, 3, Subjects, "subject"));
        }

        public async Task<Message> ShowDayOfWeekSelector(ITelegramBotClient client, CallbackQuery callbackQuery,
            ActionType actionType)
        {
            _notCheckedHomework = new Homework();

            var subjectNumber = int.Parse(callbackQuery.Data.Substring("subject".Length));

            if (_homeworks.Any(homework => homework.Subject == Subjects[subjectNumber]))
            {
                _notCheckedHomework.Subject = Subjects[subjectNumber];
                return await client.ToMessageAsync(callbackQuery.Message, "Выберите день недели:",
                    actionType, AnnouncementType.Regular,
                    keyboardMarkup: MarkupConstructor.CreateMarkup(3, 2, DayOfWeeks, "dayOfWeek",
                        new() { { "dayOfWeekBack", "К списку предметов" } }));
            }

            return await client.ToMessageAsync(callbackQuery.Message, "Ничего",
                actionType, AnnouncementType.Regular,
                keyboardMarkup: MarkupConstructor.CreateMarkup(
                    new() { { "dayOfWeekBack", "К списку предметов" } }));
        }

        public async Task<Message> ShowHomework(ITelegramBotClient client, CallbackQuery callbackQuery,
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
                    return await client.ToMessageAsync(callbackQuery.Message,
                        $"Домашнее задание по {homework.Subject.Substring(0, homework.Subject.Length - 1) + "е"} за {homework.DayOfWeek.ToLower()}",
                        actionType, AnnouncementType.Regular, deletePreviousMessage,
                        replyToMessageId: homework.MessageId,
                        keyboardMarkup: MarkupConstructor.CreateMarkup(
                            new() { { "homeworkBack", "К началу" } }));
                }
            }

            return await client.ToMessageAsync(callbackQuery.Message,
                "Ничего",
                ActionType.EditText, AnnouncementType.Regular,
                keyboardMarkup: MarkupConstructor.CreateMarkup(
                    new() { { "homeworkBack", "К началу" } }));
        }

        public async Task<Message> ShowAllHomework(ITelegramBotClient client, Message message)
        {
            return await client.ToMessageAsync(message,
                string.Join("\n", _homeworks),
                ActionType.SendText, AnnouncementType.Regular);
        }

        public async Task<Message> NewHomework(ITelegramBotClient client, Message message, List<string> textList)
        {
            if (message.Photo == null || textList.Count < 2)
            {
                return await client.ToMessageAsync(message,
                    "Некорректный ввод",
                    ActionType.SendText, AnnouncementType.Error);
            }

            if (!Subjects.Contains(textList.First().ToTitle()))
            {
                return await client.ToMessageAsync(message,
                    "Предмет указан неправильно",
                    ActionType.SendText, AnnouncementType.Error);
            }

            var subject = textList.First().ToTitle();
            var dayOfWeek = textList.Middle() != "н" ? textList[1] : DayOfWeeks[Utilities.GetDayOfWeek() - 1];
            var date = textList.Last() != "н" ? textList.Last() : DateTime.Now.ToString($"d-M-yyyy");

            _notCheckedHomework = new Homework(subject, dayOfWeek, date, message.MessageId);

            return await client.SendPhotoAsync(message.Chat.Id, message.Photo.First().FileId,
                "Картинка - <i>приложена</i> \n" +
                "Предмет - " + subject + "\n" +
                "Дата - " + date + "\n" +
                "День недели - " + dayOfWeek + "\n" +
                "Правильно?",
                replyMarkup: MarkupConstructor.CreateMarkup(1, 2, new List<string> { "Да", "Нет" }, "choice"),
                parseMode: ParseMode.Html);
        }

        public async Task<Message> SaveHomework(ITelegramBotClient client, CallbackQuery callbackQuery)
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