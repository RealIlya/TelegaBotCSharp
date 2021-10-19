using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegaBot.Models
{
    public class KbMarkup
    {
        public static TelegramBotClient Client;

        private int _messageId;
        private readonly string _text;
        private readonly InlineKeyboardMarkup _keyboardMarkup;

        public KbMarkup(string text, InlineKeyboardMarkup inlineKeyboardMarkup)
        {
            // _messageId = messageId;
            _text = text;
            _keyboardMarkup = inlineKeyboardMarkup;
        }

        public async Task<Message> SendKeyboardMarkup(Message message)
        {
            var sentMessage = await Client.SendTextMessageAsync(message.Chat.Id, _text, replyMarkup: _keyboardMarkup,
                parseMode: ParseMode.Html);
            _messageId = sentMessage.MessageId;
            return null;
        }

        public async Task<Message> EditKeyboardMarkup(CallbackQuery callbackQuery)
        {
            return await Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
                _text,
                replyMarkup: _keyboardMarkup,
                parseMode: ParseMode.Html);
        }

        public async Task RemoveKeyboardMarkup(Message message)
        {
            await Client.DeleteMessageAsync(message.Chat.Id, _messageId);
        }
    }
}