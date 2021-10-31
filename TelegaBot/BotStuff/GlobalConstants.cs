using System.Collections.Generic;

namespace TelegaBot.BotStuff
{
    public static class GlobalConstants
    {
        public static readonly List<string> DayOfWeeks = new() 
            { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };

        public static readonly List<string> Subjects = new()
        {
            "Физика", "Математика", "Информатика",
            "Русский язык", "Англ. яз", "Литература",
            "Химия", "Инженерная графика", "Биология",
            "История", "Обществознание", "География"
        };
    }
}