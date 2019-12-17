using System;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Data;
using HeuristicLab.RuntimePrediction.Common;

namespace HeuristicLab.RuntimePrediction.Preprocessing {
  class FeatureExtractor {
    internal static IEnumerable<Parameter> ExtractProblemFeatures(IEnumerable<Parameter> parameterList) {
      var parameters = parameterList.GroupBy(p => p.Name).ToDictionary(p => p.Key, p => p.First());
      if (!parameters.ContainsKey(Constants.PROBLEM_TYPE_KEY)) return new List<Parameter>();
      var problemType = (string)parameters[Constants.PROBLEM_TYPE_KEY].Value;
      try {
        if (problemType == "TravelingSalesmanProblem") {
          return ExtractTspFeatures(parameters);
        }
        return new List<Parameter> { new Parameter("ProblemSize", null, null) };
      } catch (Exception e) {
        Logger.Error($"could not extract probelm features for problem {parameters["Problem Name"].Value}", e);
        return new List<Parameter> { new Parameter("ProblemSize", null, null) };
      }
    }


    private static IEnumerable<Parameter> ExtractTspFeatures(Dictionary<string, Parameter> parameters) {
      if (parameters.ContainsKey("Coordinates")) {
        var coordinates = (DoubleMatrix)parameters["Coordinates"].Item;
        yield return new Parameter("ProblemSize", null, coordinates.Rows);
      } else if (parameters.ContainsKey("DistanceMatrix")) {
        var dm = (DoubleMatrix)parameters["DistanceMatrix"].Item;
        yield return new Parameter("ProblemSize", null, dm.Rows);
      } else {
        throw new ArgumentException("could not extract problemsize from tsp");
      }
    }
  }
}