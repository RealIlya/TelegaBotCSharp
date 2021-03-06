using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegaBot.BotStuff
{
    public static class Utilities
    {
        public static TSource Pop<TSource>(this IList<TSource> source, int index)
        {
            TSource result = source[index];
            source.RemoveAt(index);
            return result;
        }

        public static TSource Middle<TSource>(this IList<TSource> source) => source[source.Count / 2];

        public static List<string> RemoveACommand(this List<string> list) => list.GetRange(1, list.Count - 1);

        public static List<string> RemoveACommand(this string str, char separator)
        {
            var list = str.Split(separator).ToList();
            
            return list.GetRange(1, list.Count - 1);
        }

        public static string ToTitle(this string text)
        {
            if (text.Length < 0) throw new SyntaxErrorException();
            return char.ToUpper(text.FirstOrDefault()) + text.Substring(1);
        }

        public static string ToTitleAll(this string text)
        {
            if (text.Length < 0) throw new SyntaxErrorException();
            return string.Join(" ", text.Split(' ').Select(word => char.ToUpper(word.First()) + word.Substring(1)));
        }

        public static int GetDayOfWeek() => (int)DateTime.Today.DayOfWeek == 0 ? 1 : (int)DateTime.Today.DayOfWeek;

        public static bool CheckYourself(this ITelegramBotClient client, long fromId) =>
            fromId == client.GetMeAsync().Result.Id;
    }
}