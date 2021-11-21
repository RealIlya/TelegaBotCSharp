using System.Collections.Generic;
using TelegaBot.BotStuff;

namespace TelegaBot.Models
{
    public record Homework
    {
        public string Subject { get; set; }
        public string DayOfWeek { get; set; }
        public string Date { get; set; }
        public int MessageId { get; set; }
        public long ChatId { get; set; }

        public Homework()
        {
        }

        public Homework(string subject, string dayOfWeek, string date, int messageId, long chatId)
        {
            Subject = subject;
            DayOfWeek = dayOfWeek;
            Date = date;
            MessageId = messageId;
            ChatId = chatId;
        }

        public override string ToString()
        {
            return $"Date: {Date}, DoW: {DayOfWeek.ToLower()}, Subject: {Subject}, MId: {MessageId}, CId: {ChatId}";
        }
    }
}