using System;
using System.Collections.Generic;

namespace sly.lexer.fsm;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> DistinctWithPredicate<T>(this IEnumerable<T> source, Func<T, T, bool> predicate)
    {
        var items = new List<T>();
        foreach (var element in source)
        {
            bool isDuplicate = false;
            foreach (var existing in items)
            {
                if (predicate(element, existing))
                {
                    isDuplicate = true;
                    break;
                }
            }
            if (!isDuplicate)
            {
                items.Add(element);
                yield return element;
            }
        }
    }
}