using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HeuristicLab.Algorithms.EvolutionStrategy;
using HeuristicLab.Algorithms.GeneticAlgorithm;
using HeuristicLab.Algorithms.SimulatedAnnealing;
using HeuristicLab.Common;
using HeuristicLab.PluginInfrastructure;
using HeuristicLab.Problems.Knapsack;
using HeuristicLab.Problems.TravelingSalesman;
using HeuristicLab.RuntimePrediction.Analyze;
using HeuristicLab.RuntimePrediction.Common;
using HeuristicLab.RuntimePrediction.Preprocessing;
using static HeuristicLab.RuntimePrediction.Utils.Utils;

namespace HeuristicLab.RuntimePrediction {
  class Program {

    private static readonly Config config = new Config();
    static void Main(string[] args) {
      ContentManagerInitializer.Initialize();

      //LoadNecessaryAssemblies();

      var experimentPath = config.AnalyzePath + "/ML/";
      var files = new List<string> {  "Experiment_Trainingsdaten.hl","Experiment_Testdaten.hl", "Experiment_Trainingsdaten_Runs.hl", "Experiment_Testdaten_Runs.hl" }.Select(f => new FileInfo(experimentPath + f));
      var runCollections = files.Select(f => new { RunCollection = DataExtractor.ExtractRunCollectionFromHlFile(f), File = f }).ToList();
      foreach (var rc in runCollections) {
        rc.RunCollection.ForEach(run => run.Parameters.Add("trainingData", new HeuristicLab.Data.BoolValue(rc.File.Name.Contains("Training"))));
      }

      DataExtractor.SaveRunCollectionToCsv(runCollections.Select(rc => rc.RunCollection), experimentPath + "all_data.csv");

      SafeExecute(() => new ParameterAnalyzePipeline<SimulatedAnnealing, KnapsackProblem>().Run().Wait());
      SafeExecute(() => new ParameterAnalyzePipeline<GeneticAlgorithm, KnapsackProblem>().Run().Wait());
      SafeExecute(() => new ParameterAnalyzePipeline<GeneticAlgorithm, TravelingSalesmanProblem>().Run().Wait());
      SafeExecute(() => new ParameterAnalyzePipeline<EvolutionStrategy, TravelingSalesmanProblem>().Run().Wait());
      SafeExecute(() => new ParameterAnalyzePipeline<SimulatedAnnealing, TravelingSalesmanProblem>().Run().Wait());
      
      Console.WriteLine("-- end -- ");
      Console.ReadKey();
    }

    private static void LoadNecessaryAssemblies() {
      new DirectoryInfo("./")
        .GetFiles("*Problems.Instances*.dll")
        .ForEach(f => Assembly.LoadFile(f.FullName));

    }
  }
}