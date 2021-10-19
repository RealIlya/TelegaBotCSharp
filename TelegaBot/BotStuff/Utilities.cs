using System.Collections.Generic;
using System.Data;
using System.Linq;

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

        public static string ToTitle(this string str)
        {
            if (str.Length < 0) throw new SyntaxErrorException();
            return char.ToUpper(str.FirstOrDefault()) + str.Substring(1);
        }
    }
}