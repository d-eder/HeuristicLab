using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.RuntimePrediction {
  static class Utils {
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> body) {
      foreach(var item in enumerable) {
        body(item);
      }
    }

    public static bool ImplementsGenericInterface(this Type type, Type @interface) {
      return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == @interface);
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
}
