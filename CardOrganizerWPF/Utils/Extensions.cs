using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardOrganizerWPF
{
    public static class ListExtension
    {
        public static void BubbleSort(this IList o)
        {
            for(int i = o.Count - 1; i >= 0; i--)
            {
                for(int j = 1; j <= i; j++)
                {
                    object o1 = o[j - 1];
                    object o2 = o[j];
                    if(((IComparable)o1).CompareTo(o2) > 0)
                    {
                        o.Remove(o1);
                        o.Insert(j, o1);
                    }
                }
            }
        }

        public static void Sort<TSource, TValue>(this IList<TSource> source, Func<TSource, TValue> selector)
        {
            for(int i = source.Count - 1; i >= 0; i--)
            {
                for(int j = 1; j <= i; j++)
                {
                    TSource o1 = source.ElementAt(j - 1);
                    TSource o2 = source.ElementAt(j);
                    TValue x = selector(o1);
                    TValue y = selector(o2);
                    var comparer = Comparer<TValue>.Default;
                    if(comparer.Compare(x, y) > 0)
                    {
                        source.Remove(o1);
                        source.Insert(j, o1);
                    }
                }
            }
        }
    }
}
