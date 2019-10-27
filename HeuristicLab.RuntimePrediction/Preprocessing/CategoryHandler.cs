using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Data;
using HeuristicLab.Optimization;

namespace HeuristicLab.RuntimePrediction.Preprocessing {
  class CategoryHandler {
    private Dictionary<string, ISet<object>> categoryOptions = new Dictionary<string, ISet<object>>();
    private IEnumerable<IRun> runs;

    public CategoryHandler(IEnumerable<IRun> runs) {
      this.runs = runs;
      ExtractCategories();
    }

    private void ExtractCategories() {

      foreach (var run in runs) {
        foreach (var param in run.Parameters) {
          if (param.Value is StringValue sv) {
            if (!categoryOptions.TryGetValue(param.Key, out ISet<object> options)) {
              categoryOptions[param.Key] = (options = new HashSet<object>());
            }
            options.Add(sv.Value);
          }
        }
      }
    }

    public IEnumerable<Parameter> CreateCategories(Parameter param) {
      if (!categoryOptions.TryGetValue(param.Name, out ISet<object> options) || options.Count == 0) {
        yield break;
      }

      foreach (var option in options) {
        object value = param.Value;
        string name = $"is{option}_{param.Name}_Binary";

        object paramValue;
        if (value.Equals(option)) {
          paramValue = 1;
        } else {
          paramValue = 0;
        }

        yield return new Parameter(name, param.Item, paramValue) {
          IsCategory = true
        };
      }

    }
  }
}
