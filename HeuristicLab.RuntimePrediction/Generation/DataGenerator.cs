using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Problems.Instances;
using HeuristicLab.Tracing;
using SRandom = System.Random;

namespace HeuristicLab.RuntimePrediction {
  class DataGenerator<TAlgorithm, TProblem> where TAlgorithm : IAlgorithm, new() where TProblem : IProblem, new() {
    private IAlgorithmRunner runner = AlgrithmRunnerFactory.GetRunner();
    private SRandom random = new SRandom();


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

    private async Task<GenerationResult> RunAlgorithm(int number) {
      var problem = new TProblem();
      LoadRandomProblemInstance(problem);

      var algorithm = new TAlgorithm() {
        Problem = problem,
      };

      if (algorithm is EngineAlgorithm ea) {
        ea.Engine = new SequentialEngine.SequentialEngine();
      }

      //Parameterizer.SetParameters(algorithm);

      Logger.Info($"prepare algorithm {number}");
      PrintParams(algorithm);
      PrintParams(problem);
      algorithm.Prepare();
      Logger.Info($"start algorithm {number}");

      await runner.RunAlgorithm(algorithm);
      Logger.Info($"algorithm {number} finished (runtime={algorithm.ExecutionTime})");

      return new GenerationResult(number, algorithm);
    }

    private void LoadRandomProblemInstance(TProblem problem) {
      // todo

      //var handler = ProblemInstancePreparer.Create(problem);
      //if (handler.Instances.Count == 0) {
      //  throw new InvalidOperationException($"no instances found for proble {problem}: missing a reference?");
      //}
      //var instance = random.FromCollection(handler.Instances);
      //handler.SetInstance(problem, instance);
    }

    private void PrintParams(IParameterizedNamedItem item) {
      Logger.Info($"Parameters for {item.Name}:");
      foreach (var param in item.Parameters) {
        Logger.Info($"{param.Name} = {param.ActualValue}");
      }
      Logger.Info("");
    }
  }
}