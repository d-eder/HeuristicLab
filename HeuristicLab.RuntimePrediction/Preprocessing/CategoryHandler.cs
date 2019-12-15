using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;

namespace HeuristicLab.RuntimePrediction.Preprocessing {

  interface ICategoryHandler {
    void ExtractCategories(IDictionary<string, IItem> parameters);
    IEnumerable<CategoryParameter> CreateCategories(Parameter param);
  }

  class CategoryHandler : ICategoryHandler {
    private Dictionary<string, ISet<object>> categoryOptions = new Dictionary<string, ISet<object>>();
    private IEnumerable<IRun> runs;

    public CategoryHandler() { }
      public CategoryHandler(IEnumerable<IRun> runs) {
      foreach (var run in runs) {
        ExtractCategories(run.Parameters);
      }
    }



    public void ExtractCategories(IDictionary<string, IItem> parameters) {
      foreach (var param in parameters) {
        if (param.Value is StringValue sv) {
          if (!categoryOptions.TryGetValue(param.Key, out ISet<object> options)) {
            categoryOptions[param.Key] = (options = new HashSet<object>());
          }
          options.Add(sv.Value);
        }
      }
    }


    public IEnumerable<CategoryParameter> CreateCategories(Parameter param) {
      if (!categoryOptions.TryGetValue(param.Name, out ISet<object> options) || options.Count == 0) {
        yield break;
      }

      foreach (var option in options) {
        object value = param.Value;
        string name = $"is{option}";//_{param.Name}_Binary";

        object paramValue;
        if (value.Equals(option)) {
          paramValue = 1;
        } else {
          paramValue = 0;
        }

        yield return new CategoryParameter(param, name, paramValue);
      }
    }
  }
}
