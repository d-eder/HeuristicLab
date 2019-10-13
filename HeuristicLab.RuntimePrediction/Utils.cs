using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public static T FromCollection<T>(this Random random, ICollection<T> values) {
      return values.ElementAt(random.Next(0, values.Count));
    }
  }


  class ConsoleLogger : ILogger {
    public void Info(string msg) {
      Console.WriteLine(msg);
    }
  }

  interface ILogger {
    void Info(string msg);

  }

  class LockedRandom {
    private Random random = new Random();

    internal int Next(int from, int to) {
      lock (this) {
        return random.Next(from, to);
      }
    }

    internal bool NextBool() {
      return NextDouble() >= 0.5;
    }

    internal double NextDouble() {
      lock (this) {
        return random.NextDouble();
      }
    }

    internal T FromCollection<T>(ICollection<T> values) {
      lock (this) {
        return random.FromCollection(values);
      }
    }
  }
}
