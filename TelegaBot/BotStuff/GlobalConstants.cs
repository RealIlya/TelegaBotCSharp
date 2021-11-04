using System.Collections.Generic;

namespace TelegaBot.BotStuff
{
    public abstract class GlobalConstants
    {
        protected readonly List<string> DayOfWeeks;

        protected readonly List<string> Subjects;

        protected GlobalConstants()
        {
            DayOfWeeks = new() 
                { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };
            
            Subjects = new()
            {
                "Физика", "Математика", "Информатика",
                "Рус.яз", "Англ.яз", "Литература",
                "Химия", "Инженерная графика", "Биология",
                "История", "Обществознание", "География"
            };
        }
    }
}