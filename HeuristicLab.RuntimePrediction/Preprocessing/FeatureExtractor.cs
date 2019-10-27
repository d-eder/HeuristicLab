using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;

namespace HeuristicLab.RuntimePrediction.Preprocessing {
  class FeatureExtractor {
    internal static IEnumerable<Parameter> ExtractProblemFeatures(IEnumerable<Parameter> parameterList) {
      var parameters = parameterList.ToDictionary(p => p.Name, p => p);
      var problemType = (string)parameters["Problem Type"].Value;
      try {
        if (problemType == "TravelingSalesmanProblem") {
          return ExtractTspFeatures(parameters);
        }
        return new Parameter[0];
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