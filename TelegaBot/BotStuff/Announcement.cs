using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegaBot.BotStuff
{
    public enum AnnouncementType
    {
        Info,
        Error,
    }

    public enum ActionType
    {
        SendText,
        EditText,
        SendPhoto,
        EditCaption
    }

    public static class Announcement
    {
        public static async Task<Message> ToMessageAsync(
            this ITelegramBotClient client,
            Message message,
            string text,
            ActionType actionType,
            AnnouncementType? announcementType,
            bool deletePreviousMessage = false,
            int? replyToMessageId = null,
            InlineKeyboardMarkup keyboardMarkup = null)
        {
            await DeletePreviousMessage(client, message, /*message.ReplyToMessage != null*/ deletePreviousMessage);

            return await Sender(client, message, text, actionType, announcementType, replyToMessageId, keyboardMarkup,
                null);
        }

        public static async Task<Message> ToMessageAsync(this ITelegramBotClient client,
            Message message,
            string text,
            ActionType actionType,
            bool deletePreviousMessage = false,
            int? replyToMessageId = null,
            InlineKeyboardMarkup keyboardMarkup = null)
        {
            await DeletePreviousMessage(client, message, /*message.ReplyToMessage != null*/ deletePreviousMessage);

            return await Sender(client, message, text, actionType, null, replyToMessageId, keyboardMarkup, null);
        }

        public static async Task<Message> ToMessageAsync(this ITelegramBotClient client,
            Message message,
            InputOnlineFile photo,
            string caption,
            ActionType actionType,
            bool deletePreviousMessage = false,
            int? replyToMessageId = null,
            InlineKeyboardMarkup keyboardMarkup = null)
        {
            await DeletePreviousMessage(client, message, /*message.ReplyToMessage != null*/ deletePreviousMessage);

            return await Sender(client, message, caption, actionType, null, replyToMessageId, keyboardMarkup, photo);
        }

        private static Task<Message> Sender(ITelegramBotClient client, Message message, string text,
            ActionType actionType, AnnouncementType? announcementType, int? replyToMessageId,
            InlineKeyboardMarkup keyboardMarkup, InputOnlineFile photo)
        {
            keyboardMarkup ??= MarkupConstructor.CreateMarkup();

            // var a = message.Text != null
            //     ? new EditMessageTextRequest(message.Chat.Id, message.MessageId, text) as RequestBase<Message>
            //     : new EditMessageCaptionRequest(message.Chat.Id, message.MessageId) {Caption = text};
            //
            // Message b = client.MakeRequestAsync(request: a).Result;

            return actionType switch
            {
                ActionType.SendText => client.SendTextMessageAsync(message.Chat.Id,
                    WhatAnnouncementType(announcementType, text),
                    replyMarkup: keyboardMarkup,
                    replyToMessageId: replyToMessageId,
                    parseMode: ParseMode.Html),
                ActionType.EditText => client.EditMessageTextAsync(message.Chat.Id, message.MessageId,
                    WhatAnnouncementType(announcementType, text),
                    replyMarkup: keyboardMarkup,
                    parseMode: ParseMode.Html),
                ActionType.SendPhoto => client.SendPhotoAsync(message.Chat.Id, photo,
                    WhatAnnouncementType(announcementType, text),
                    replyMarkup: keyboardMarkup,
                    replyToMessageId: replyToMessageId,
                    parseMode: ParseMode.Html),
                _ => null
            };
        }

        private async static Task DeletePreviousMessage(ITelegramBotClient client, Message message,
            bool deletePreviousMessage)
        {
            if (deletePreviousMessage)
                await client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
        }

        private static string WhatAnnouncementType(AnnouncementType? announcementType, string text)
        {
            return announcementType switch
            {
                AnnouncementType.Error => $"<b><u>{text}</u></b>",
                AnnouncementType.Info => $"<b>{text}</b>",
                _ => text
            };
        }
    }
}