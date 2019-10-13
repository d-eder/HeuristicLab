using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Optimization;
using HeuristicLab.RuntimePrediction.DataGeneration;

namespace HeuristicLab.RuntimePrediction {
  class RuntimePredictionService {

    private ILogger logger = new ConsoleLogger();
    private Config config = new Config();

    static RuntimePredictionService() {
      ContentManager.Initialize(new PersistenceContentManager());
    }

    public void GenerateRawData<TAlgorithm, TProblem>(int count) where TAlgorithm : IAlgorithm, IStorableContent, new() where TProblem : IProblem, new() {
      var generator = new DataGenerator<TAlgorithm, TProblem>(logger);

      generator.GenerateData(count)
        .GetTasksLazy()
        .ForEach(a => SaveAlgorithm(a));
    }

    private async void SaveAlgorithm(Task<GenerationResult> algorithmTask) {
      var result = await algorithmTask;
      var algorithm = result.Algorithm;
      var now = DateTime.Now.ToString("dd.MM.yyyy-HH.mm.ss");

      var dir = Directory.CreateDirectory(Path.Combine(config.RawDataPath, algorithm.GetType().Name, algorithm.Problem.GetType().Name));
      var fileName = Path.Combine(dir.FullName, $"{now}-{result.Nr}.hl");
      
      ContentManager.Save((IStorableContent) algorithm, fileName, true);
      logger.Info($"write file {fileName}");
    }
  }
}
