using System.Collections.Generic;
using HeuristicLab.Core;

namespace HeuristicLab.RuntimePrediction.Preprocessing {
  public interface IParameterProcessor {
    ICollection<Parameter> ProcessParameters(IEnumerable<KeyValuePair<string, IItem>> parameters);
  }
}