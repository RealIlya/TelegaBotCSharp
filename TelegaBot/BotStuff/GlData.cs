using System.Collections.Generic;

namespace TelegaBot.BotStuff
{
    public struct GlConsts
    {
        public const string ROOT_PATH = @"C:\Users\Admin\Desktop\TelegaBot\TelegaBot\Img\";

        public static readonly List<string> DayOfWeeks = new()
            { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };

        public static readonly List<string> Subjects = new()
        {
            "Физика", "Математика", "Информатика",
            "Рус.яз", "Англ.яз", "Литература",
            "Химия", "Инженерная графика", "Биология",
            "История", "Обществознание", "География"
        };

        public const string HAVE_NOT_A_PHOTO = "Картинка не приложена\n";
        public const string NOT_ENOUGH_ARGUMENTS = "Слишком мало аргументов, образец:\n";
    }
}