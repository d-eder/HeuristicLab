using System;
using System.Collections.Generic;
using HeuristicLab.Core;

namespace HeuristicLab.RuntimePrediction.Analyze {
  public interface IParameterFetcher {
    IDictionary<string, IItem> GetParameters(Type algorithmType, Type problemType);
  }
}