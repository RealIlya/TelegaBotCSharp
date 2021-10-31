﻿using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegaBot.BotStuff
{
    public static class MarkupConstructor
    {
        public static InlineKeyboardMarkup CreateMarkup<T>(int rows, int columns,
            List<T> content,
            string callbackPrefix,
            Dictionary<string, string> optionalButtons = null)
        {
            var keyboard = new List<List<InlineKeyboardButton>>();

            for (int i = 0; i < rows; i++)
            {
                keyboard.Add(new List<InlineKeyboardButton>());
                for (int j = 0; j < columns; j++)
                {
                    keyboard[i].Add(InlineKeyboardButton.WithCallbackData(content[i * columns + j].ToString(),
                        $"{callbackPrefix}{i * columns + j}"));
                }
            }

            if (optionalButtons != null)
                foreach (var item in optionalButtons)
                {
                    keyboard.Add(new List<InlineKeyboardButton>()
                        { InlineKeyboardButton.WithCallbackData(item.Value, item.Key) });
                }

            keyboard.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData("Закрыть", "close") });

            return new InlineKeyboardMarkup(keyboard);
        }

        public static InlineKeyboardMarkup CreateMarkup()
        {
            var keyboard = new List<List<InlineKeyboardButton>>
                { new() { InlineKeyboardButton.WithCallbackData("Закрыть", "close") } };
            return new InlineKeyboardMarkup(keyboard);
        }
    }
}