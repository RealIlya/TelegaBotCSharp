using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TelegaBot.AnswerCallbacks;
using TelegaBot.Commands;
using TelegaBot.Interfaces;
using TelegaBot.Models;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegaBot.BotStuff
{
    internal class Handlers
    {
        private static readonly List<string> _TimetableCallbacks;
        private static readonly List<string> _SubjectCallbacks;
        private static readonly List<string> _DayOfWeekCallbacks;

        private static readonly Dictionary<string, ICommand> _Commands = new()
        {
            { "/help", new ShowHelpCommand() },
            { "/work", new ShowDutyCommand() },
            { "/time", new ShowTimeCommand() },
            { "/timetable", new ShowTimetableSelectorCommand() },
            // { "/hw", null },
            // { "newhw", null },

            { "/echo", new EchoCommand() },
            { "/black", new ShowBlackCommand() },
            { "/insult", new InsultCommand() },

            // { "/allhw", null }
        };

        private static readonly Dictionary<string, IAnswer> _AnswerCallbacks = new()
        {
            { string.Join("", Enumerable.Range(0, 7).Select(n => $"timetable{n}")), new ShowTimetableAnswer() },
            { "timetableBack", new ShowTimetableSelectorAnswer() }
        };

        static Handlers()
        {
            _TimetableCallbacks = new List<string>();
            for (int i = 0; i < 7; i++) _TimetableCallbacks.Add($"timetable{i}");

            _SubjectCallbacks = new List<string>();
            for (int i = 0; i < 16; i++) _SubjectCallbacks.Add($"subject{i}");

            _DayOfWeekCallbacks = new List<string>();
            for (int i = 0; i < 7; i++) _DayOfWeekCallbacks.Add($"dayOfWeek{i}");
        }

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException =>
                    $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient client, Update update,
            CancellationToken cancellationToken)
        {
            Task handler = update.Type switch
            {
                UpdateType.Message => OnMessageReceived(client, update.Message),
                UpdateType.EditedMessage => OnMessageReceived(client, update.EditedMessage),
                UpdateType.CallbackQuery => OnCallbackQueryReceived(client, update.CallbackQuery),
                _ => UnknownUpdateHandler(client, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(client, exception, cancellationToken);
            }
        }

        private static async Task OnMessageReceived(ITelegramBotClient client, Message message)
        {
            var messageText = message.Text ?? message.Caption;
            if (messageText?.FirstOrDefault() != '/') return;
            var textList = messageText.Split(' ').ToList();

            Task<Message> command;

            var commandString = textList.Pop(0).Split('@').First();
            if (_Commands[_Commands.ContainsKey(commandString) ? commandString : "/help"]
                .CanExecute(message, out (string text, AnnouncementType type) error))
            {
                command = _Commands[commandString].Execute(client, message);
            }
            else
            {
                command = client.ToMessageAsync(message, error.text, ActionType.SendText, error.type);
            }

            /*
            command = textList.Pop(0).Split('@').First() switch
            {
                // "/help" => OtherCommands.ShowHelp(client, message),
                // "/work" => OtherCommands.ShowDuty(client, message),
                // "/time" => OtherCommands.ShowTime(client, message),
                // "/timetable" => TimetableCommands.ShowTimetableSelector(client, message),
                // "/black" => OtherCommands.ShowBlack(client, message, textList),
                "/hw" => HomeworkCommands.ShowSubjectSelector(client, message),
                "/newhw" => HomeworkCommands.NewHomework(client, message, textList),
                // "/echo" => OtherCommands.Echo(client, message, textList),
                "/allhw" => HomeworkCommands.ShowAllHomework(client, message),
                // "/insult" => OtherCommands.Insult(client, message, textList),
                _ => null
            };
            */

            if (command != null) await command;
        }

        private static async Task OnCallbackQueryReceived(ITelegramBotClient client, CallbackQuery callbackQuery)
        {
            var message = callbackQuery.Message;
            var callbackData = callbackQuery.Data ?? String.Empty;

            Task answer = null;

            if (callbackData != "close")
            {
                foreach (var callbacksKey in _AnswerCallbacks.Keys
                    .Where(callbacksKey => callbacksKey.Contains(callbackData)))
                {
                    if (_AnswerCallbacks[callbacksKey]
                        .CanExecute(callbackQuery, out (string text, AnnouncementType type) error))
                    {
                        answer = _AnswerCallbacks[callbacksKey].Execute(client, callbackQuery);
                        break;
                    }

                    answer = client.ToMessageAsync(message, error.text, ActionType.SendText, error.type);
                }
            }
            else
            {
                answer = client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            }

            // foreach (var answerCallbacksKey in _AnswerCallbacks.Keys)
            // {
            //     foreach (var element in answerCallbacksKey)
            //     {
            //         if (callbackQuery.Data == element)
            //         {
            //             answer = _AnswerCallbacks[answerCallbacksKey].Execute(client, callbackQuery);
            //             break;
            //         }
            //
            //         if (callbackQuery.Data == "timetableBack")
            //         {
            //             answer
            //         }
            //     }
            // }


            if (_SubjectCallbacks.Contains(callbackQuery.Data))
            {
                answer = HomeworkCommands.ShowDayOfWeekSelector(client, callbackQuery);
            }
            else if (_DayOfWeekCallbacks.Contains(callbackQuery.Data))
            {
                answer = HomeworkCommands.ShowHomework(client, callbackQuery);
            }
            else
                answer = callbackQuery.Data switch
                {
                    // "timetableBack" => TimetableCommands.ShowTimetableSelector(client, callbackQuery.Message),
                    "dayOfWeekBack" => HomeworkCommands.ShowSubjectSelector(client, callbackQuery.Message),
                    "homeworkBack" => HomeworkCommands.ShowSubjectSelector(client, callbackQuery.Message),
                    "choice0" => HomeworkCommands.SaveHomework(client, callbackQuery.Message),
                    "choice1" => client.ToMessageAsync(callbackQuery.Message, "Отменено",
                        ActionType.SendText, AnnouncementType.Info, true),
                    // "close" => client.DeleteMessageAsync(callbackQuery.Message.Chat.Id,
                    //     callbackQuery.Message.MessageId),
                    _ => null
                };

            if (answer != null) await answer;
        }

        private static Task UnknownUpdateHandler(ITelegramBotClient client, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }
    }
}