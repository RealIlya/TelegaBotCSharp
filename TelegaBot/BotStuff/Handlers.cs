using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
        public static Commands _commands;

        private static readonly List<string> _timetableCallbacks;

        static Handlers()
        {
            _timetableCallbacks = new List<string>();
            for (int i = 0; i < 9; i++) _timetableCallbacks.Add($"t{i}");
        }

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException =>
                    $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            Task handler = update.Type switch
            {
                UpdateType.Message => OnMessageReceived(botClient, update.Message),
                // UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage),
                UpdateType.CallbackQuery => OnCallbackQueryReceived(botClient, update.CallbackQuery),
                // UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery),
                // UpdateType.ChosenInlineResult => OnChosenInlineResultReceived(botClient, update.ChosenInlineResult),
                _ => UnknownUpdateHandler(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task OnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            if (message.Type != MessageType.Text || message.Text?.FirstOrDefault() != '/') return;
            var textList = message.Text.Split(' ').ToList();

            var command = textList.Pop(0).Split('@').First() switch
            {
                "/help" => _commands.ShowHelp(message),
                "/work" => _commands.ShowDuty(message),
                "/time" => _commands.ShowTime(message),
                "/timetable" => _commands.ShowTimetableSelector(message),
                "/black" => _commands.ShowBlackFrame(message),
                "/hw" => _commands.ShowHomeWork(message),
                "/newhw" => _commands.NewHomeWork(message),
                "/echo" => _commands.Echo(message, textList),
                _ => _commands.ByDefault(message)
            };

            await command;
        }

        private static async Task OnCallbackQueryReceived(ITelegramBotClient botClient,
            CallbackQuery callbackQuery)
        {
            if (_timetableCallbacks.Contains(callbackQuery.Data))
            {
                await _commands.ShowTimetable(callbackQuery);
            }
            else
            {
                await _commands.MoveBack(callbackQuery);
            }
        }

        private static Task UnknownUpdateHandler(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }
    }
}