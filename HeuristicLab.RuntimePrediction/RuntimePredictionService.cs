using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Optimization;
using HeuristicLab.RuntimePrediction.Common;
using HeuristicLab.RuntimePrediction.Preprocessing;

namespace HeuristicLab.RuntimePrediction {
  class RuntimePredictionService {

    private const string TARGET_LABEL = "Execution Time";
    private readonly Config config = new Config();

    private HlDataExtractor extractor = new HlDataExtractor();

    static RuntimePredictionService() {
      ContentManagerInitializer.Initialize();
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

  }
}
