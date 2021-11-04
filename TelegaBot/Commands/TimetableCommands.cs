﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TelegaBot.BotStuff;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegaBot.Commands
{
    public class TimetableCommands : GlobalConstants
    {
        private Dictionary<string, JToken> _timetable;

        private string _lastActiveUri;

        public TimetableCommands()
        {
            _lastActiveUri = GetUri();
            _timetable = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(GetTimetable());
        }

        private string GetUri()
        {
            var web = new HtmlWeb();
            var htmlDoc = web.Load(@"https://lyceum.nstu.ru/rasp/schedule.html");
            var uri = htmlDoc.DocumentNode.SelectSingleNode("//script").GetAttributeValue("src", "");

            return uri;
        }

        private string GetTimetable()
        {
            using var siteClient = new HttpClient();
            var timetable = siteClient.GetStringAsync(@"https://lyceum.nstu.ru/rasp/" + _lastActiveUri).Result
                .Replace("// nika_data.js;  description: schedule in JSON format\r\n" +
                         "// this file automatically generated by Nika-Soft(c) products\r\n \r\n" +
                         "var NIKA=\r\n", "").Trim()
                .Replace(";", "");

            return timetable;
        }

        #region Commands block

        public async Task<Message> ShowTimetableSelector(ITelegramBotClient client, Message message,
            ActionType actionType)
        {
            return await client.ToMessageAsync(message, "Выберите день недели:",
                actionType, AnnouncementType.Regular,
                keyboardMarkup: MarkupConstructor.CreateMarkup(3, 2, DayOfWeeks, "timetable",
                    new Dictionary<string, string>() { { "timetable6", "Сегодня" } }));
        }

        public async Task<Message> ShowTimetable(ITelegramBotClient client, CallbackQuery callbackQuery)
        {
            if (_lastActiveUri != GetUri())
            {
                _lastActiveUri = GetUri();
                await client.ToMessageAsync(callbackQuery.Message, "Подождите...",
                    ActionType.EditText, AnnouncementType.Regular,
                    keyboardMarkup: MarkupConstructor.CreateMarkup());
                Console.WriteLine("Отпарсено заново");
                _timetable = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(GetTimetable());
            }

            // const string path = "50";
            var flag = true;
            var lesson = 1;
            var callbackQueryDataNumber = int.Parse(callbackQuery.Data.Substring("timetable".Length)) != 6
                ? int.Parse(callbackQuery.Data.Substring("timetable".Length))
                : Utilities.GetDayOfWeek() - 1;

            var result = $"<b><u>{DayOfWeeks[callbackQueryDataNumber]}</u></b>\n";
            var classSchedule = ((JProperty)_timetable["CLASS_SCHEDULE"].First()).Value["029"];

            foreach (JToken item in classSchedule)
            {
                var name = ((JProperty)item).Name;
                if (name.First().ToString() != (callbackQueryDataNumber + 1).ToString()) continue;
                if (name.Last() == '3' && flag) lesson += 2;

                var subjects = (from JProperty subject in _timetable["SUBJECTS"]
                    from ssg in classSchedule[name]["s"]
                    where subject.Name == ssg.ToString()
                    select _timetable["SUBJECTS"][subject.Name].ToString().ToTitle()).ToList();

                var teachers = (from JProperty teacher in _timetable["TEACHERS"]
                    from tsg in classSchedule[name]["t"]
                    where teacher.Name == tsg.ToString()
                    select _timetable["TEACHERS"][teacher.Name].ToString()).ToList();

                var rooms = (from JProperty room in _timetable["ROOMS"]
                    from rsg in classSchedule[name]["r"]
                    where room.Name == rsg.ToString()
                    select _timetable["ROOMS"][room.Name].ToString()).ToList();

                // if ((lesson - 1) % 2 == 0) result += $"<b>Пара {lesson / 2 + 1}</b>\n";

                result +=
                    (((lesson - 1) % 2 == 0) ? $"<b>Пара {lesson / 2 + 1}</b>\n {lesson++}" : lesson++) +
                    $". {string.Join(" / ", subjects)}({string.Join(" / ", rooms)})\n" +
                    $"    {string.Join(" / ", teachers)}\n";
                flag = false;
            }

            return await client.ToMessageAsync(callbackQuery.Message, result,
                ActionType.EditText, AnnouncementType.Regular,
                keyboardMarkup: MarkupConstructor.CreateMarkup(
                    new Dictionary<string, string>() { { "timetableBack", "Назад" } }));
        }

        #endregion
    }
}