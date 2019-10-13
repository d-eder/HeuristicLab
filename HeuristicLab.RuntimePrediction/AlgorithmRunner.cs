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
    public Task RunAlgorithm(IAlgorithm algorithm) {
#if DEBUG
      algorithm.Start();
      return Task.CompletedTask;
#else
      return algorithm.StartAsync();
#endif
    }
  }

  interface IAlgorithmRunner {
    Task RunAlgorithm(IAlgorithm algorithm);
  }
}
