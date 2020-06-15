using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Common.Utils
{
    public static class Fn
    {
        public static KeyValuePair<string, object> Kvp(string key, object val)
        {
            return new KeyValuePair<string, object>(key, val);
        }

        public static IEnumerable<KeyValuePair<string, object>> IdFilter(string id)
        {
            return new[] { Kvp("_id", id) };
        }

        public static IEnumerable<KeyValuePair<string, object>> IdFilter(IEnumerable<string> ids)
        {
            return new[] { Kvp("_id", ids) };
        }

        public static double? RoundNullable(double? x, int dp)
        {
            return x.HasValue ? Math.Round(x.Value, dp) : x;
        }
        
        public static TDst Clone<TSrc, TDst>(TSrc source, TDst target) where TDst : TSrc
        {
            var bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var sourceProps = new HashSet<string>(source.GetType().GetProperties(bf).Where(p => p.CanRead).Select(pi => pi.Name));
            var targetProps = target.GetType().GetProperties(bf).Where(p => p.CanWrite);
            foreach (var prop in targetProps.Where(pi => sourceProps.Contains(pi.Name)))
                prop.SetValue(target, prop.GetValue(source));
            return target;
        }

        public static IEnumerable<T> YieldNone<T>()
        {
            yield break;
        }
    }
}
