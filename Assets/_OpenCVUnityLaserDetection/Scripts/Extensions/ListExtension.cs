using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.BulletDecals.Scripts.Extensions
{
    /// <summary>
    /// Generic List extension methods
    /// </summary>
    public static class ListExtension
    {
        public static T RandomItem<T>(this List<T> list) where T : class
        {
            if (list != null && list.Count > 0)
            {
                var randomIndex = Random.Range(0, list.Count);
                return list[randomIndex];    
            }
            return null;
        }

        public static List<T> Clone<T>(this List<T> list)
        {
            return list.Select(item => item).ToList();
        }

        /// <summary>
        /// Shifts item down
        /// </summary>
        public static void ShiftItemDown<T>(this List<T> list, int itemIndex)
        {
            int count = list.Count;
            if (count == itemIndex + 1)
                return;
            T item = list[itemIndex];
            list.Remove(item);
            list.Insert(itemIndex + 1, item);
        }

        /// <summary>
        /// Shifts item up
        /// </summary>
        public static void ShiftItemUp<T>(this List<T> list, int itemIndex)
        {
            if (itemIndex == 0)
                return;
            T item = list[itemIndex];
            list.Remove(item);
            list.Insert(itemIndex - 1, item);
        }

        public static void SwapFirstAndLast<T>(this List<T> list)
        {
            if (list != null)
            {
                var count = list.Count;
                if (count > 1)
                {
                    SwapItems(list, 0, count - 1);
                }
            }
        }

        public static void SwapItems<T>(this List<T> list, int indexA, int indexB)
        {
            if (list != null)
            {
                T tmp = list[indexA];
                list[indexA] = list[indexB];
                list[indexB] = tmp;
            }
        }
    }
}