using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Common;
using HeuristicLab.Optimization;

namespace HeuristicLab.RuntimePrediction {
  public class HlDataExtractor {

    internal Task<RunCollection> ExtractRunCollectionFromHlFile(FileInfo f) {
      return Task.Run(() => {
        try {
          Logger.Info($"Extract Runs from {f}");
          var content = ContentManager.Load(f.FullName);
          return ExtractRunCollection(content);
        } catch (Exception e) {
          Logger.Error($"could not load file {f.Name}", e);
          return new RunCollection();
        }
      });
    }

    private static RunCollection ExtractRunCollection(IStorableContent content) {
      if (content is IAlgorithm a) {
        return a.Runs;
      } else if (content is RunCollection r) {
        return r;
      } else {
        throw new ArgumentException($"cannot extract runncollection from type {content.GetType()}");
      }
    }
  }
}
