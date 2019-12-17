using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HeuristicLab.Optimization;
using HeuristicLab.RuntimePrediction.Experiments;
using HeuristicLab.RuntimePrediction.Preprocessing;

namespace HeuristicLab.RuntimePrediction.Analyze {
  class ParameterAnalyzePipeline<TAlgorithm, TProblem> where TAlgorithm : IAlgorithm where TProblem : IProblem {
    private readonly Config config = new Config();

    private IParameterFetcher parameterFetcher = ServiceFactory.GetParameterFetcher();
    private IParameterProcessor processor = ServiceFactory.GetParameterProcessor();
    private IExperimentCreator experimentCreator = ServiceFactory.GetExperimentCreator();
    private IParameterAnalyzerService analyzerService = ServiceFactory.GetParameterAnalyzerService();
    private IAlgorithmRunner runner = AlgrithmRunnerFactory.GetRunner();

    private Type algorithmType = typeof(TAlgorithm);
    private Type problemType = typeof(TProblem);

    private DirectoryInfo AnalysePath => new DirectoryInfo(config.AnalyzePath);
    private FileInfo HlFile => new FileInfo(AnalysePath.FullName + "/" + Name + ".hl");
    private string Name => experimentCreator.GetExperimentName(algorithmType, problemType);

    public async Task Run() {

      AnalyzeExperiment analyzeExpr = null;

      if (HlFile.Exists) {
        try {
          analyzeExpr = await analyzerService.GetExperimentWithFilename(HlFile.FullName);
          if (analyzeExpr != null) {
            Logger.Info("loaded experiment");
            PrintParameters(analyzeExpr);
          }
        } catch (Exception e) {
          File.Delete(HlFile.FullName);
          Logger.Error("could not load experiment from file", e);
        }
      }

      if (analyzeExpr == null) {
        analyzeExpr = await CreateExperiments();
      }

      var expr = analyzeExpr.Experiment;

      await RunExperiment(expr);

      await analyzerService.SaveExperiment(analyzeExpr);
      Logger.Info("experiment saved");
      ExtractData(analyzeExpr);
    }

    private void ExtractData(AnalyzeExperiment analyzeExpr) {
      string basePath = $"{AnalysePath.FullName }/";
      var runs = DataExtractor.ExtractRunCollection(analyzeExpr.Experiment);
      DataExtractor.SaveRunCollectionToCsv(runs, $"{basePath}{HlFile.Name}.csv");

      foreach (var param in analyzeExpr.GeneratedParameters.Select(p => p.parameterName).Distinct()) {
        var runsWithParam = new RunCollection(analyzeExpr.Experiment.Optimizers.Where(o => o.Name.Contains("-" + param)).SelectMany(o => o.Runs));
        //var runsWithParam = new RunCollection(runs.Where(r => r.Algorithm.Name.Contains(param)));
        if (runsWithParam.Count == 0) {
          throw new Exception($"could not find runs with parameter {param}");
        }
        DataExtractor.SaveRunCollectionToCsv(runsWithParam, $"{basePath}{HlFile.Name}-{param}.csv");
      }
    }

    private static async Task RunExperiment(Experiment expr) {
      expr.Optimizers.ForEach(o => {
        if (o is EngineAlgorithm ea && ea.Engine == null) {
          ea.Engine = new SequentialEngine.SequentialEngine();
        }
      });

      if (expr.Runs.Count == expr.Optimizers.Count) {
        Logger.Info("experiment already contains all runs");
      } else {
        expr.NumberOfWorkers = Environment.ProcessorCount;
        var numberRuns = expr.Optimizers.Count;
        Logger.Info($"starting experiment {expr.Name} (optimizers={expr.Optimizers.Count}, worker={expr.NumberOfWorkers})");
        expr.Prepare(true);
        var task = expr.StartAsync();
        int nrRuns = 0;
        while (!task.IsCompleted) {
          if (nrRuns != expr.Runs.Count) {
            nrRuns = expr.Runs.Count;
            Logger.Info($"{nrRuns}/{expr.Optimizers.Count} finished");
          }
          await Task.Delay(5000);
        }
        Logger.Info($"experiment {expr.Name} finished");
      }
    }

    public async Task<AnalyzeExperiment> CreateExperiments() {
      if (HlFile.Exists) await analyzerService.GetExperimentWithFilename(HlFile.FullName);

      var plainParameters = parameterFetcher.GetParameters(algorithmType, problemType);
      var processedParameters = processor.ProcessParameters(plainParameters);
      var expr = await experimentCreator.CreateExperimentForParameters(algorithmType, problemType, processedParameters);

      PrintParameters(expr);
      expr.File = HlFile;
      await analyzerService.SaveExperiment(expr);
      Console.WriteLine($"experiment created in {HlFile}");
      return expr;
    }

    private static void PrintParameters(AnalyzeExperiment expr) {
      Logger.Info("Generated Parameters: ");
      expr.GeneratedParameters
          .GroupBy(p => p.parameterName, p => p.value)
          .Select(g => new KeyValuePair<string, string>(g.Key, string.Join(", ", g.ToArray())))
          .ForEach(kvp => {
            Logger.Info($"{kvp.Key}: {kvp.Value}");
          });

      Logger.Info("");

    }
  }
}
