using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TelegaBot.BotStuff;
using TelegaBot.Interfaces;
using TelegaBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace TelegaBot.Commands
{
    public class ShowDutyCommand : ICommand
    {
        private int _lastSeenDay;
        private readonly List<Student> _students;
        private List<Student> _todayDuties;

        public ShowDutyCommand()
        {
            _lastSeenDay = DateTime.Today.Day;

            _students = File.Exists("Students.json")
                ? JsonConvert.DeserializeObject<List<Student>>(File.ReadAllText("Students.json"))
                : new List<Student>();
            _todayDuties = GetTodayStudents(_students);
        }

        public async Task<Message> Execute(ITelegramBotClient client, Message message)
        {
            var presentDay = DateTime.Today.Day;
            if (_lastSeenDay != presentDay)
            {
                _lastSeenDay = presentDay;
                _todayDuties = GetTodayStudents(_students);
            }

            return await client.ToMessageAsync(message,
                $"Дежурят сегодня - <b>{string.Join(" и ", _todayDuties)}</b>",
                ActionType.SendText);
        }

        public bool CanExecute(Message message, out (string, AnnouncementType) error)
        {
            error = (null, AnnouncementType.Info);
            return true;
        }

        private static List<Student> GetTodayStudents(IReadOnlyList<Student> students)
        {
            var random = new Random();
            return new List<Student>()
            {
                students[random.Next(0, students.Count / 2)],
                students[random.Next(students.Count / 2, students.Count)]
            };
        }
    }
}