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
    public class TimetableCommands
    {
        private readonly Dictionary<string, JToken> _timetable;

        private string _lastActiveUri;

        public TimetableCommands()
        {
            _lastActiveUri = GetUri();
            try
            {
                _timetable = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(GetTimetable());
            }
            catch (Exception e)
            {
                Console.WriteLine("Parsing is not successful");
            }
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

        public async Task<Message> ShowTimetableSelector(ITelegramBotClient client, Message message)
        {
            return await client.ToMessageAsync(message, "Выберите день недели:",
                ActionType.Send, AnnouncementType.Regular,
                keyboardMarkup: MarkupConstructor.CreateMarkup(3, 2, GlobalConstants.DayOfWeeks, "timetable",
                    new Dictionary<string, string>() { { "timetable6", "Сегодня" } }));
        }

        public async Task<Message> ShowTimetableSelector(ITelegramBotClient client, CallbackQuery callbackQuery)
        {
            return await client.ToMessageAsync(callbackQuery.Message, "Выберите день недели:",
                ActionType.Edit, AnnouncementType.Regular,
                keyboardMarkup: MarkupConstructor.CreateMarkup(3, 2, GlobalConstants.DayOfWeeks, "timetable",
                    new Dictionary<string, string>() { { "timetable6", "Сегодня" } }));
        }

        public async Task<Message> ShowTimetable(ITelegramBotClient client, CallbackQuery callbackQuery)
        {
            if (_lastActiveUri != GetUri())
            {
                _lastActiveUri = GetUri();
            }
            const string path = "48";
            var flag = true;
            var lesson = 1;
            var presentDayOfWeek = (int)DateTime.Today.DayOfWeek == 0 ? 1 : (int)DateTime.Today.DayOfWeek;
            var callbackQueryDataNumber = int.Parse(callbackQuery.Data.Substring("timetable".Length)) != 6
                ? int.Parse(callbackQuery.Data.Substring("timetable".Length))
                : presentDayOfWeek - 1;

            var result = $"<b><u>{GlobalConstants.DayOfWeeks[callbackQueryDataNumber]}</u></b>\n";

            foreach (JToken item in _timetable["CLASS_SCHEDULE"][path]["029"])
            {
                var name = ((JProperty)item).Name;
                if (name.FirstOrDefault().ToString() != (callbackQueryDataNumber + 1).ToString()) continue;
                if (name.LastOrDefault() == '3' && flag) lesson += 2;

                var subjects = (from JProperty subject in _timetable["SUBJECTS"]
                    from ssg in _timetable["CLASS_SCHEDULE"][path]["029"][name]["s"]
                    where subject.Name == ssg.ToString()
                    select _timetable["SUBJECTS"][subject.Name].ToString().ToTitle()).ToList();

                var teachers = (from JProperty teacher in _timetable["TEACHERS"]
                    from tsg in _timetable["CLASS_SCHEDULE"][path]["029"][name]["t"]
                    where teacher.Name == tsg.ToString()
                    select _timetable["TEACHERS"][teacher.Name].ToString()).ToList();

                var rooms = (from JProperty room in _timetable["ROOMS"]
                    from rsg in _timetable["CLASS_SCHEDULE"][path]["029"][name]["r"]
                    where room.Name == rsg.ToString()
                    select _timetable["ROOMS"][room.Name].ToString()).ToList();

                if ((lesson - 1) % 2 == 0) result += $"<b>Пара {lesson / 2 + 1}</b>\n";

                result +=
                    $"{lesson++}. {string.Join(" / ", subjects)}({string.Join(" / ", rooms)})\n" +
                    $"    {string.Join(" / ", teachers)}\n";
                flag = false;
            }

            return await client.ToMessageAsync(callbackQuery.Message, result,
                ActionType.Edit, AnnouncementType.Regular,
                keyboardMarkup: MarkupConstructor.CreateMarkup<string>(0, 0, null, null,
                    new Dictionary<string, string>() { { "timetableBack", "Назад" } }));
        }

        #endregion
    }
}