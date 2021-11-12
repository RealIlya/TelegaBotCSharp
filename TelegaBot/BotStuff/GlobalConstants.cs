using System.Collections.Generic;

namespace TelegaBot.BotStuff
{
    public abstract class GlobalConstants
    {
        protected static readonly List<string> DayOfWeeks = new() 
            { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };

        protected static readonly List<string> Subjects = new()
        {
            "Физика", "Математика", "Информатика",
            "Рус.яз", "Англ.яз", "Литература",
            "Химия", "Инженерная графика", "Биология",
            "История", "Обществознание", "География"
        };

        protected const string HaveNotAPhoto = "Картинка не приложена\n";
        protected const string NotEnoughArguments = "Слишком мало аргументов, образец:\n";
    }
}