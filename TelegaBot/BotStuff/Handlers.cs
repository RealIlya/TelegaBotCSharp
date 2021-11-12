using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TelegaBot.Commands;
using TelegaBot.Models;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
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
                // UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery),
                // UpdateType.ChosenInlineResult => OnChosenInlineResultReceived(botClient, update.ChosenInlineResult),
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

            var command = textList.Pop(0).Split('@').First() switch
            {
                "/help" => OtherCommands.ShowHelp(client, message),
                "/work" => OtherCommands.ShowDuty(client, message),
                "/time" => OtherCommands.ShowTime(client, message),
                "/timetable" => TimetableCommands.ShowTimetableSelector(client, message, ActionType.SendText),
                "/black" => OtherCommands.ShowBlack(client, message, textList),
                "/hw" => HomeworkCommands.ShowSubjectSelector(client, message, ActionType.SendText),
                "/newhw" => HomeworkCommands.NewHomework(client, message, textList),
                "/echo" => OtherCommands.Echo(client, message, textList),
                "/allhw" => HomeworkCommands.ShowAllHomework(client, message),
                "/insult" => OtherCommands.Insult(client, message, textList),
                _ => null
            };

            if (command != null) await command;
        }

        private static async Task OnCallbackQueryReceived(ITelegramBotClient client,
            CallbackQuery callbackQuery)
        {
            Task answer;

            if (_TimetableCallbacks.Contains(callbackQuery.Data))
            {
                answer = TimetableCommands.ShowTimetable(client, callbackQuery);
            }
            else if (_SubjectCallbacks.Contains(callbackQuery.Data))
            {
                answer = HomeworkCommands.ShowDayOfWeekSelector(client, callbackQuery, ActionType.EditText);
            }
            else if (_DayOfWeekCallbacks.Contains(callbackQuery.Data))
            {
                answer = HomeworkCommands.ShowHomework(client, callbackQuery, ActionType.SendText, true);
            }
            else
                answer = callbackQuery.Data switch
                {
                    "timetableBack" => TimetableCommands.ShowTimetableSelector(client, callbackQuery.Message,
                        ActionType.EditText),
                    "dayOfWeekBack" => HomeworkCommands.ShowSubjectSelector(client, callbackQuery.Message,
                        ActionType.EditText),
                    "homeworkBack" => HomeworkCommands.ShowSubjectSelector(client, callbackQuery.Message,
                        ActionType.SendText, true),
                    "choice0" => HomeworkCommands.SaveHomework(client, callbackQuery),
                    "choice1" => client.ToMessageAsync(callbackQuery.Message, "Отменено",
                        ActionType.SendText, AnnouncementType.Info, true),
                    "close" => client.DeleteMessageAsync(callbackQuery.Message.Chat.Id,
                        callbackQuery.Message.MessageId),
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