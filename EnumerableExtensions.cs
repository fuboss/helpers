using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.Tools
{
    public static class EnumerableExtensions
    {
        public static T GetRandomItem<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0) throw new ArgumentException("Argument null or empty");
            if (list.Count == 1) return list[0];

            return list.ElementAt(UnityEngine.Random.Range(0, list.Count));
        }

        public static T GetRandomItem<T>(this IEnumerable<T> list)
        {
            if (list == null || !list.Any()) throw new ArgumentException("Argument null or empty");
            if (list.Count() == 1) return list.First();

            return list.ElementAt(UnityEngine.Random.Range(0, list.Count()));
        }

        /// <summary>
        /// Get <see cref="count"/> unique elements from <see cref="list"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetRandomItems<T>(this IEnumerable<T> list, int count)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (list.Count() <= count) return list;
            var sourceList = new List<T>(list);
            var resultList = new List<T>(count);

            for (var i = 0; i < count; i++)
            {
                var selected = sourceList.GetRandomItem();
                sourceList.Remove(selected);
                resultList.Add(selected);
            }

            return resultList;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static IEnumerator Empty
        {
            get
            {
                yield break;
            }
        }
    }
}
