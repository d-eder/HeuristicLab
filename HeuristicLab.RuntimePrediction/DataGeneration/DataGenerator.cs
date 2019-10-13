using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Problems.Instances;
using HeuristicLab.RuntimePrediction.Parameters;
using HeuristicLab.Tracing;

namespace HeuristicLab.RuntimePrediction.DataGeneration {
  class DataGenerator<TAlgorithm, TProblem> where TAlgorithm : IAlgorithm, new() where TProblem : IProblem, new() {

    private ILogger logger;
    private IAlgorithmRunner runner = AlgrithmRunnerFactory.GetRunner();
    private Random random = new Random();

    public DataGenerator(ILogger logger) {
      this.logger = logger;
    }

    public TaskQueue<Task<GenerationResult>> GenerateData(int count) {
      var taskQueue = new TaskQueue<Task<GenerationResult>>(runner.ParallismCount);
      new Task(() => {
        for (int i = 0; i < count; i++) {
          taskQueue.Add(RunAlgorithm(i + 1));
        }
        taskQueue.SetFinished();
      }, TaskCreationOptions.LongRunning).Start();

      return taskQueue;
    }

    //public IEnumerable<Task<GenerationResult>> GenerateData(int count) {
    //  for (int i = 0; i < count; i++) {
    //    var task = RunAlgorithm(i + 1);
    //    yield return task;
    //  }
    //}

    private async Task<GenerationResult> RunAlgorithm(int number) {
      var problem = new TProblem();
      LoadRandomProblemInstance(problem);

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

    private void LoadRandomProblemInstance(TProblem problem) {
      var handler = ProblemInstancePreparer.Create(problem);
      if (handler.Instances.Count == 0) {
        throw new InvalidOperationException($"no instances found for proble {problem}: missing a reference?");
      }
      var instance = random.FromCollection(handler.Instances);
      handler.SetInstance(problem, instance);
    }

    private void PrintParams(IParameterizedNamedItem item) {
      logger.Info($"Parameters for {item.Name}:");
      foreach (var param in item.Parameters) {
        logger.Info($"{param.Name} = {param.ActualValue}");
      }
      logger.Info("");
    }
  }
}