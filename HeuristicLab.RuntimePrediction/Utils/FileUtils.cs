using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.RuntimePrediction {
  static class FileUtils {

    public static IEnumerable<FileInfo> GetFilesRecursive(DirectoryInfo dir) {
      foreach (var file in dir.GetFiles()) {
        yield return file;
      }

      foreach (var d in dir.GetDirectories()) {
        foreach(var f in GetFilesRecursive(d)) {
          yield return f;
        }
      }
    }
  }
}
