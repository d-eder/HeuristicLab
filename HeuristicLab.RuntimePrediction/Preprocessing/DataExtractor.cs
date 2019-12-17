using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Common;
using HeuristicLab.Optimization;

namespace HeuristicLab.RuntimePrediction.Preprocessing {
  public class DataExtractor {
    internal const string TARGET_LABEL = "Execution Time";

    public static void SaveRunCollectionToCsv(RunCollection runs, string file) => SaveRunCollectionToCsv(new List<RunCollection> { runs }, file);

    public static void SaveRunCollectionToCsv(IEnumerable<RunCollection> runs, string file, Action<Data> dataManipulator = null) {
      DataPreprocessor processor = new DataPreprocessor(TARGET_LABEL);
      runs.ForEach(r => processor.AddRuns(r));
      processor.Process();
      dataManipulator?.Invoke(processor.Data);
      var outputFile = new FileInfo(file);
      Logger.Info("writing csv file " + outputFile.Name);
      CsvUtil.Write(outputFile, processor.GetDataAsDynamic());
    }

    public static void ExtractDataFromHlFiles(string fileName, string hlFileDirctory, string csvPath, bool allInOneCsv) {
      var experimentPath = new DirectoryInfo(hlFileDirctory);
      var files = FileUtils
          .GetFilesRecursive(experimentPath);

      var runCollections = (from f in files
                            where f.Name.EndsWith(".hl")
                            select new { FileName = f.Name, Runs = ExtractRunCollectionFromHlFile(f) }
                           ).ToList();

      runCollections.ForEach(r => {
        string file = csvPath + "/" + r.FileName.Substring(0, r.FileName.Length - 3) + ".csv";
        SaveRunCollectionToCsv(r.Runs, file);
      });

      SaveRunCollectionToCsv(runCollections.Select(r => r.Runs), csvPath + "/" + "data.csv");
    }

    public static RunCollection ExtractRunCollectionFromHlFile(FileInfo f) {
      try {
        Logger.Info($"extract runs from {f.Name}");
        var content = ContentManager.Load(f.FullName);
        return ExtractRunCollection(content);
      } catch (Exception e) {
        Logger.Error($"could not load file {f.Name}", e);
        return new RunCollection();
      }
    }

    static internal RunCollection ExtractRunCollection(IStorableContent content) {
      if (content is IAlgorithm a) {
        return a.Runs;
      } else if (content is RunCollection r) {
        return r;
      } else if (content is Experiment e) {
        return e.Runs;
      } else {
        throw new ArgumentException($"cannot extract runncollection from type {content.GetType()}");
      }
    }
  }
}
