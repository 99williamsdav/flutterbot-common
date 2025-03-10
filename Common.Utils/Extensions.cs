using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Utils
{
    public static class Extensions
    {
        private const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static List<T> AsList<T>(this IEnumerable<T> collection)
        {
            return collection as List<T> ?? collection.ToList();
        }

        public static int IndexOf<T>(this IEnumerable<T> collection, Predicate<T> predicate)
        {
            var i = 0;
            foreach (var x in collection)
            {
                if (predicate(x))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        public static int DayIndexFromMonday(this DayOfWeek dow)
        {
            return dow == DayOfWeek.Sunday ? 6 : (int)dow - 1;
        }

        public static T ToEnum<T>(this string enumVal) where T : struct, IConvertible
        {
            return enumVal != null ? (T)Enum.Parse(typeof(T), enumVal, true) : new T();
        }

        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static int GetQuarter(this DateTime date)
        {
            return (date.Month + 2) / 3;
        }

        public static string EncodeBase36(this long value)
        {
            string result = string.Empty;
            int targetBase = Digits.Length;

            do
            {
                result = Digits[Convert.ToInt32(value % targetBase)] + result;
                value = value / targetBase;
            }
            while (value > 0);

            return result;
        }

        public static string EncodeBase36(this int value)
        {
            return EncodeBase36(Convert.ToInt64(value));
        }

        public static string ToPrettyString(this TimeSpan value)
        {
            return value.Hours > 0 ? $"{value.TotalHours:N0}h {value.Minutes}m {value.Seconds}s"
                : value.Minutes > 0 ? $"{value.TotalMilliseconds:N0}m {value.Seconds}s"
                : value.Seconds > 0 ? $"{value.TotalSeconds:N3}s"
                : $"{value.TotalMilliseconds:N0}ms";
        }
    }
}
