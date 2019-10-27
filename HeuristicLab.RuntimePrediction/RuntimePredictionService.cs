using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Optimization;
using HeuristicLab.RuntimePrediction.Preprocessing;

namespace HeuristicLab.RuntimePrediction {
  class RuntimePredictionService {

    private const string TARGET_LABEL = "Execution Time";
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
      var files = FileUtils
          .GetFilesRecursive(experimentPath);

      var runCollections = from f in files
                           where f.Name.EndsWith(".hl")
                           select new { File = f, Runs = extractor.ExtractRunCollectionFromHlFile(f) };

      var dataPath = new DirectoryInfo(experimentPath.FullName).Parent.FullName;

      var allDataProcessor = new DataPreprocessor(TARGET_LABEL);

      runCollections.Select(async (x) => {
        var runs = await x.Runs;
        if (runs.Count == 0)
          return runs;
        Logger.Info("creating data from " + x.File.Name);
        var processor = new DataPreprocessor(TARGET_LABEL, runs);
        processor.Process();
        var outputFile = new FileInfo(dataPath + "\\" + x.File.Name + "_data.csv");
        Logger.Info("writing file " + outputFile.Name);
        CsvUtil.Write(outputFile, processor.GetDataAsDynamic());
        return runs;

      }).ToList()
      .ForEach(t => allDataProcessor.AddRuns(t.Result));

      allDataProcessor.Process();
      var fullOutputFile = new FileInfo(dataPath + "\\all_data.csv");
      Console.WriteLine("writing file " + fullOutputFile.Name);
      CsvUtil.Write(fullOutputFile, allDataProcessor.GetDataAsDynamic());
    }

  }
}
