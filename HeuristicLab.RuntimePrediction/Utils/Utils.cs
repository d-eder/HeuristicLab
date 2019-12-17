using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.RuntimePrediction.Utils {
  static class Utils {
    public static void SafeExecute( Action action) {
      try {
        action();
      }catch(Exception e) {
        Logger.Error("could not execute action", e);
      }
    }
  }
}
