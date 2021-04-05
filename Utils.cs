using System;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBot
{
    public static class Utils
    {
        static Random rnd = new Random();
        public static List<T> Shuffle<T>(this List<T> list)
        {
            return list.OrderBy(l => rnd.Next()).ToList();
        }
    }
}