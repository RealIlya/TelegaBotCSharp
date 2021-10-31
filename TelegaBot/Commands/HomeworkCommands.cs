using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public class HomeworkCommands
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

        public async Task<Message> ShowSubjectSelector(ITelegramBotClient client, Message message)
        {
            return await client.ToMessageAsync(message, "Выберите предмет:",
                ActionType.Send, AnnouncementType.Regular,
                keyboardMarkup: MarkupConstructor.CreateMarkup(4, 3, GlobalConstants.Subjects, "subject"));
        }

        public async Task<Message> ShowDayOfWeekSelector(ITelegramBotClient client, CallbackQuery callbackQuery)
        {
            _notCheckedHomework = new Homework();

            var subjectNumber = int.Parse(callbackQuery.Data.Substring("subject".Length));

            if (_homeworks.Any(homework => homework.Subject == GlobalConstants.Subjects[subjectNumber]))
            {
                _notCheckedHomework.Subject = GlobalConstants.Subjects[subjectNumber];
                return await client.ToMessageAsync(callbackQuery.Message, "Выберите день недели:",
                    ActionType.Edit, AnnouncementType.Regular,
                    keyboardMarkup: MarkupConstructor.CreateMarkup(3, 2, GlobalConstants.DayOfWeeks, "dayOfWeek",
                        new Dictionary<string, string>() { { "dayOfWeek6", "Сегодня" } }));
            }

            return await client.ToMessageAsync(callbackQuery.Message, "Ничего",
                ActionType.Edit, AnnouncementType.Regular);
        }

        public async Task<Message> ShowHomework(ITelegramBotClient client, CallbackQuery callbackQuery)
        {
            var dayOfWeekNumber = int.Parse(callbackQuery.Data.Substring("dayOfWeek".Length));

            if (_homeworks.Any(homework => homework.DayOfWeek == GlobalConstants.DayOfWeeks[dayOfWeekNumber]))
            {
                _notCheckedHomework.DayOfWeek = GlobalConstants.DayOfWeeks[dayOfWeekNumber];
            }

            // var currentHomework = _homeworks.Where(homework => homework.Subject == _notCheckedSubject && homework.DayOfWeek == _notCheckedDayOfWeek)
            //     .Select(homework => new List<string>() {homework.DayOfWeek, homework.Subject}).ToList().First();

            foreach (var homework in _homeworks)
            {
                if (homework.Subject == _notCheckedHomework.Subject &&
                    homework.DayOfWeek == _notCheckedHomework.DayOfWeek)
                {
                    return await client.ToMessageAsync(callbackQuery.Message,
                        $"Домашнее задание по {homework.Subject.Substring(0, homework.Subject.Length - 1) + "е"} за {homework.DayOfWeek}",
                        ActionType.Send, AnnouncementType.Regular, true,
                        replyToMessageId: homework.MessageId);
                }
            }

            return await client.ToMessageAsync(callbackQuery.Message, "Ничего",
                ActionType.Edit, AnnouncementType.Regular);
        }

        public async Task<Message> ShowAllHomework(ITelegramBotClient client, Message message)
        {
            return await client.ToMessageAsync(message, string.Join("\n", _homeworks),
                ActionType.Send, AnnouncementType.Regular);
        }

        public async Task<Message> NewHomework(ITelegramBotClient client, Message message, List<string> textList)
        {
            if (message.Photo == null || textList.Count < 2)
            {
                return await client.ToMessageAsync(message, "Некорректный ввод",
                    ActionType.Send, AnnouncementType.Error);
            }

            if (!GlobalConstants.Subjects.Contains(textList.First().ToTitle()))
            {
                return await client.ToMessageAsync(message, "Предмет указан неправильно",
                    ActionType.Send, AnnouncementType.Error);
            }

            var presentDayOfWeek = (int)DateTime.Today.DayOfWeek == 0 ? 1 : (int)DateTime.Today.DayOfWeek;
            
            var subject = textList.First().ToTitle();
            var dayOfWeek = textList[1] != "н" ? textList[1] : GlobalConstants.DayOfWeeks[presentDayOfWeek - 1];
            var date = textList.Last() != "н" ? textList.Last() : DateTime.Now.ToString($"d-M-yyyy");
            // var date =  textList.Last() ?? DateTime.Now.ToString($"d-M-yyyy"); 

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

            await client.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);

            return await client.ToMessageAsync(callbackQuery.Message, "Сохранено",
                ActionType.Send, AnnouncementType.Info);
        }

        #endregion
    }
}