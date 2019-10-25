using System;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.RuntimePrediction {

  public static class Logger {
    public static void Info(string msg) {
      Console.WriteLine(msg);
    }

    public static void Error(string msg, Exception e) {
      var color = Console.ForegroundColor;
      var m = msg + (e != null ? ": " + e.ToString() : "");
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine(m);
      Console.ForegroundColor = color;
    }
  }
}
