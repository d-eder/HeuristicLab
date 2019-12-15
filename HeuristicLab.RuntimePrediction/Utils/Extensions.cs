using System;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Core;
using SRandom = System.Random;

namespace HeuristicLab.RuntimePrediction {
  static class Extensions {
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> body) {
      foreach (var item in enumerable) {
        body(item);
      }
    }

    public static bool ImplementsGenericInterface(this Type type, Type @interface) {
      return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == @interface);
    }

    public static T FromCollection<T>(this SRandom random, ICollection<T> values) {
      return values.ElementAt(random.Next(0, values.Count));
    }

    public static List<T> FillWithNull<T>(this List<T> list, int count) where T : class {
      while (list.Count() < count) {
        list.Add(null);
      }
      return list;
    }

    public static IEnumerable<T> AppendAll<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> addFunction) {
      foreach (var item in items) {
        yield return item;
        foreach (var addedItem in addFunction(item)) {
          yield return addedItem;
        }
      }
    }

    public static bool IsType(this IItem item, Type subType) {
      try {
        return item.GetType() == subType.MakeGenericType(item.GetType().GetGenericArguments());
      } catch (ArgumentException e) {
        return false;
      }
    }

    public static object GetUnderlyingValue(this IItem item) {
      dynamic d = item;
      try {
        return d.Value;
      }catch(Exception e) {
        return item;
      }
    }
  }
}
