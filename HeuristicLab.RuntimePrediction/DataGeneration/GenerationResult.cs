using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Optimization;

namespace HeuristicLab.RuntimePrediction.DataGeneration {
  class GenerationResult {
    public GenerationResult(int nr, IAlgorithm algorithm) {
      Nr = nr;
      Algorithm = algorithm;
    }
    public int Nr { get; private set; }
    public IAlgorithm Algorithm { get; private set; }

  }
}
