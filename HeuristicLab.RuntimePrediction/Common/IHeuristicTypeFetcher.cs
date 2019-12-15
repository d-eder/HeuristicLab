using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeuristicLab.RuntimePrediction {
  public interface IHeuristicTypeFetcher {
    Task<IEnumerable<Type>> GetAlgorithmTypes();
    Task<IEnumerable<Type>> GetProblemTypes();
  }
}