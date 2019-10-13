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
using HeuristicLab.RuntimePrediction.DataGeneration;

namespace HeuristicLab.RuntimePrediction {
  class Program {
    static void Main(string[] args) {
      LoadNecessaryAssemblies();
      new RuntimePredictionService()
        .GenerateRawData<GeneticAlgorithm, TravelingSalesmanProblem>(1000);
      Console.ReadKey();
    }

    private static void LoadNecessaryAssemblies() {
      new DirectoryInfo("./")
        .GetFiles("*Problems.Instances*.dll")
        .ForEach(f => Assembly.LoadFile(f.FullName));
    }
  }
}