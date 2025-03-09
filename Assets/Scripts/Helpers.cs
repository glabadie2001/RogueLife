using System.Collections.Generic;
using System;

public static class Helpers
{
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T element in source)
        {
            action(element);
        }
    }
}
