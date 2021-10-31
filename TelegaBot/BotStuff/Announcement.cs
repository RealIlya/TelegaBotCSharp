using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegaBot.BotStuff
{
    public enum AnnouncementType
    {
        Error,
        Warning,
        Info,
        Regular
    }

    public enum ActionType
    {
        Send,
        Edit
    }

    public static class Announcement
    {
        public static async Task<Message> ToMessageAsync(this ITelegramBotClient client, Message message, string text,
            ActionType actionType, AnnouncementType announcementType, bool deletePreviousMessage = false,
            InlineKeyboardMarkup keyboardMarkup = null, int? replyToMessageId = null)
        {
            keyboardMarkup ??= MarkupConstructor.CreateMarkup();

            if (deletePreviousMessage)
            {
                await client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            }

            var notification = actionType switch
            {
                ActionType.Send => announcementType switch
                {
                    AnnouncementType.Error => client.SendTextMessageAsync(
                        message.Chat.Id, $"<b><u>{text}</u></b>", replyMarkup: keyboardMarkup,
                        replyToMessageId: replyToMessageId,
                        parseMode: ParseMode.Html),
                    AnnouncementType.Warning => client.SendTextMessageAsync(message.Chat.Id,
                        $"<b>{text}</b>", replyMarkup: keyboardMarkup,
                        replyToMessageId: replyToMessageId,
                        parseMode: ParseMode.Html),
                    AnnouncementType.Info => client.SendTextMessageAsync(message.Chat.Id,
                        $"<b>{text}</b>", replyMarkup: keyboardMarkup,
                        replyToMessageId: replyToMessageId,
                        parseMode: ParseMode.Html),
                    AnnouncementType.Regular => client.SendTextMessageAsync(message.Chat.Id,
                        text, replyMarkup: keyboardMarkup,
                        replyToMessageId: replyToMessageId,
                        parseMode: ParseMode.Html),
                    _ => null
                },
                ActionType.Edit => announcementType switch
                {
                    AnnouncementType.Error => client.EditMessageTextAsync(message.Chat.Id, message.MessageId,
                        $"<b><u>{text}!</u></b>", replyMarkup: keyboardMarkup,
                        parseMode: ParseMode.Html),
                    AnnouncementType.Warning => client.EditMessageTextAsync(message.Chat.Id, message.MessageId,
                        $"<b>{text}!</b>", replyMarkup: keyboardMarkup,
                        parseMode: ParseMode.Html),
                    AnnouncementType.Info => client.EditMessageTextAsync(message.Chat.Id, message.MessageId,
                        $"<b>{text}.</b>", replyMarkup: keyboardMarkup,
                        parseMode: ParseMode.Html),
                    AnnouncementType.Regular => client.EditMessageTextAsync(message.Chat.Id, message.MessageId,
                        text, replyMarkup: keyboardMarkup,
                        parseMode: ParseMode.Html),
                    _ => null
                },
                _ => null
            };

            return await notification;
        }
    }
}