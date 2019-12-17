using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Core;

namespace HeuristicLab.RuntimePrediction.Preprocessing {
  class ParameterProcessor : IParameterProcessor {

    private ICategoryHandler categoryHandler;

    public const string PROBLEM_NAME_PARAM = "Problem Name";
    public const string ALGORITHM_NAME_PARAM = "Algorithm Name";

    private ISet<string> ignoreParams = new HashSet<string> { "Seed", "SetSeedRandomly", "Maximization", "UseDistanceMatrix", "BestKnownQuality" };

    public ParameterProcessor(ICategoryHandler categoryHandler) {
      this.categoryHandler = categoryHandler;
    }

    public ICollection<Parameter> ProcessParameters(IEnumerable<KeyValuePair<string, IItem>> parameters) {
      var convertedParameters = ConvertParameters(
        parameters.Where(p => !ignoreParams.Contains(p.Key) && !p.Key.Contains(".")) // todo these are ignored now
        ).ToDictionary(p => p.Name);

      var validParams = new List<Parameter>();

      foreach (var kvp in convertedParameters) {
        var p = kvp.Value;
        ConvertInvalidParamValue(p);
        if (IsCategoryParameter(p)) {
          var categoryParameters = categoryHandler.CreateCategories(p);
          
          // can overrdie existing parameters
          if (p.Name.Contains(".")) {
            var paramNames = categoryParameters.Select(p => p.Name).ToHashSet();
            validParams.RemoveAll(v => paramNames.Contains(v.Name));
          }
          validParams.AddRange(categoryParameters);

        } else {
          validParams.Add(p);
        }
      }

      validParams.AddRange(FeatureExtractor.ExtractProblemFeatures(validParams.ToList()));
      return validParams;
    }

    private IEnumerable<Parameter> ConvertParameters(IEnumerable<KeyValuePair<string, IItem>> parameters) {
      foreach (var p in parameters) {
        yield return new Parameter(p.Key, p.Value, ConvertItemToValue(p.Value));
      }
    }

    private object ConvertItemToValue(IItem item) {
      return ItemConverter.GetItemValue(item);
    }


    private bool IsCategoryParameter(Parameter p) {
      return p.ParamValue.ValueType == typeof(string) && p.Name != ALGORITHM_NAME_PARAM && p.Name != PROBLEM_NAME_PARAM;
    }

    private void ConvertInvalidParamValue(Parameter parameter) {
      if (parameter.Value is TimeSpan ts) {
        parameter.Value = ts.TotalSeconds;
      }
    }
  }
}
