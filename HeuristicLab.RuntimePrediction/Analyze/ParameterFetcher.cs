using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Core;
using HeuristicLab.Optimization;
using HeuristicLab.RuntimePrediction.Common;

namespace HeuristicLab.RuntimePrediction.Analyze {


  class ParameterFetcher : IParameterFetcher {
    private IDictionary<string, IItem> GetParameters(IEnumerable<IParameter> parameters) {
      var dict = parameters.ToDictionary(k => k.Name, v => (IItem)v);
      return dict;
    }

    public IDictionary<string, IItem> GetParameters(Type algorithmType, Type problemType) {
      var algorithm = (IAlgorithm)Activator.CreateInstance(algorithmType);
      var problem = (IProblem)Activator.CreateInstance(problemType);
      algorithm.Problem = problem;
      var dict = GetParameters(algorithm.Parameters.Union(problem.Parameters));

      if (!dict.ContainsKey(Constants.PROBLEM_TYPE_KEY)) {
        dict[Constants.PROBLEM_TYPE_KEY] = problem;
      }

      if (!dict.ContainsKey(Constants.ALGORITHM_TYPE_KEY)) {
        dict[Constants.ALGORITHM_TYPE_KEY] = algorithm;
      }

      return dict;
    }
  }
}
