using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Optimization;

namespace HeuristicLab.RuntimePrediction {

  static class AlgrithmRunnerFactory {
    public static IAlgorithmRunner GetRunner() {
      return new LocalAlgorithmRunner();
    }
  }


  class LocalAlgorithmRunner : IAlgorithmRunner {
    public int ParallismCount => Environment.ProcessorCount;

    public Task RunAlgorithm(IAlgorithm algorithm) {
      return algorithm.StartAsync();
    }
  }

  interface IAlgorithmRunner {
    Task RunAlgorithm(IAlgorithm algorithm);
    int ParallismCount { get; }
  }
}
