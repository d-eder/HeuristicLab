using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeuristicLab.Core;
using HeuristicLab.Optimization;
using HeuristicLab.RuntimePrediction.Parameters;
using HeuristicLab.Tracing;

namespace HeuristicLab.RuntimePrediction.DataGeneration {
  class DataGenerator<TAlgorithm, TProblem> where TAlgorithm : IAlgorithm, new() where TProblem : IProblem, new() {

    private ILogger logger;
    private IAlgorithmRunner runner = AlgrithmRunnerFactory.GetRunner();
   
    public DataGenerator(ILogger logger) {
      this.logger = logger;
    }

    public IList<Task<GenerationResult>> GenerateData(int count) {
      var tasks = new List<Task<GenerationResult>>();
      for (int i = 0; i < count; i++) {
        tasks.Add(RunAlgorithm(i+1));
      }
      return tasks;
    }

    private async Task<GenerationResult> RunAlgorithm(int number) {
      var problem = new TProblem();
      var algorithm = new TAlgorithm() {
        Problem = problem,
      };

      if (algorithm is EngineAlgorithm ea) {
        ea.Engine = new SequentialEngine.SequentialEngine();
      }

      Parameterizer.SetParameters(algorithm);

      logger.Info($"prepare algorithm {number}");
      PrintParams(algorithm);
      PrintParams(problem);
      algorithm.Prepare();
      logger.Info($"start algorithm {number}");

      await runner.RunAlgorithm(algorithm);
      logger.Info($"algorithm {number} finished (runtime={algorithm.ExecutionTime})");

      return new GenerationResult(number, algorithm);
    }

    private void PrintParams(IParameterizedNamedItem item) {
      logger.Info($"Parameters for {item.Name}:");
      foreach(var param in item.Parameters) {
        logger.Info($"{param.Name} = {param.ActualValue}");
      }
      logger.Info("");
    }
  }
}