namespace TelegaBot.Models
{
    public record Homework
    {
        public string Subject { get; set; }
        public string DayOfWeek { get; set; }
        public string Date { get; set; }
        public int MessageId { get; set; }

        public Homework()
        {
        }

        public Homework(string subject, string dayOfWeek, string date, int messageId)
        {
            Subject = subject;
            DayOfWeek = dayOfWeek;
            Date = date;
            MessageId = messageId;
        }

        public override string ToString()
        {
            return $"{Date}, {DayOfWeek}, {Subject}, {MessageId}";
        }

        public void Deconstruct(out string subject, out string dayOfWeek, out string date, out int messageId)
        {
            subject = Subject;
            dayOfWeek = DayOfWeek;
            date = Date;
            messageId = MessageId;
        }
    }
}