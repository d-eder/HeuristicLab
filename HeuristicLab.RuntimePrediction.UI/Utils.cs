using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.RuntimePrediction.UI {
  public static class Utils {
    public static void AddAll<T>(this ObservableCollection<T> col, IEnumerable<T> list) {
      foreach (var elem in list) {
        col.Add(elem);
      }
    }
  }
}
