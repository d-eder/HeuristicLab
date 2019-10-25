using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Optimization;

namespace HeuristicLab.RuntimePrediction {
  class RuntimePredictionService {

    private readonly Config config = new Config();

    static RuntimePredictionService() {
      ContentManager.Initialize(new PersistenceContentManager());
    }

    public void GenerateRawData<TAlgorithm, TProblem>(int count) where TAlgorithm : IAlgorithm, IStorableContent, new() where TProblem : IProblem, new() {
      var generator = new DataGenerator<TAlgorithm, TProblem>();

      generator.GenerateData(count)
        .GetTasksLazy()
        .ForEach(a => SaveAlgorithm(a));
    }

    private async void SaveAlgorithm(Task<GenerationResult> algorithmTask) {
      var result = await algorithmTask;
      var algorithm = result.Algorithm;
      var now = DateTime.Now.ToString("dd.MM.yyyy-HH.mm.ss");

      var dir = Directory.CreateDirectory(Path.Combine(config.RawDataPath, algorithm.GetType().Name, algorithm.Problem.GetType().Name));
      var fileName = Path.Combine(dir.FullName, $"{now}-{result.Nr}-{Guid.NewGuid()}.hl");

      ContentManager.Save((IStorableContent)algorithm, fileName, true);
      Logger.Info($"write file {fileName}");
    }

    internal void ExtractDataFromHlFiles() {
      var extractor = new HlDataExtractor();

      var experimentPath = new DirectoryInfo(config.ExperimentPath);

      var preparer = new DataPreparer();
      var fullData = new Data();

      var files = FileUtils
          .GetFilesRecursive(experimentPath);

      var runCollections = from f in files
                           where f.Name.EndsWith(".hl")
                           select new { File = f, Runs = extractor.ExtractRunCollectionFromHlFile(f) };

      runCollections.ForEach(async (x) => {
        var runs = await x.Runs;
        var data = preparer.CreateDataFromRuns(runs);
        var outputFile = new FileInfo(experimentPath.FullName + "\\" + x.File.Name + "_data.csv");
        CsvUtil.Write(outputFile, data.Header, data.Values);
        preparer.AddRuns(fullData, new[] { runs });
      });

      var fullOutputFile = new FileInfo(experimentPath.FullName + "\\all_data.csv");
      CsvUtil.Write(fullOutputFile, fullData.Header, fullData.Values);
    }
  }
}
