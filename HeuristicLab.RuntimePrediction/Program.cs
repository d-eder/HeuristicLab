using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Algorithms.GeneticAlgorithm;
using HeuristicLab.Problems.TravelingSalesman;
using HeuristicLab.RuntimePrediction.DataGeneration;

namespace HeuristicLab.RuntimePrediction {
  class Program {
    static void Main(string[] args) {
      new RuntimePredictionService()
        .GenerateRawData<GeneticAlgorithm, TravelingSalesmanProblem>(1);
      Console.ReadKey();
    }
  }
}