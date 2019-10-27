using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Algorithms.GeneticAlgorithm;
using HeuristicLab.PluginInfrastructure;
using HeuristicLab.Problems.TravelingSalesman;

namespace HeuristicLab.RuntimePrediction {
  class Program {
    static void Main(string[] args) {
      LoadNecessaryAssemblies();
      var predicitonService = new RuntimePredictionService();
      //predicitonService.GenerateRawData<GeneticAlgorithm, TravelingSalesmanProblem>(1000);
      //predicitonService.PreprocessData();
      predicitonService.ExtractDataFromHlFiles();
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