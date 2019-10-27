//using System.Collections.Generic;
//using System.Linq;
//using HeuristicLab.Core;
//using HeuristicLab.Data;
//using HeuristicLab.Optimization;

//namespace HeuristicLab.RuntimePrediction {
//  internal partial class DataPreparer {
//    class CategoryHandler {
//      private Dictionary<string, ISet<string>> categoryOptions = new Dictionary<string, ISet<string>>();
//      public IReadOnlyDictionary<string, ISet<string>> CategoryOptions { get => categoryOptions; }

//      public void ExtractCategories(RunCollection runs) {

//        foreach (var run in runs) {
//          foreach (var param in run.Parameters) {
//            if (param.Value is StringValue sv) {
//              if (!categoryOptions.TryGetValue(param.Key, out ISet<string> options)) {
//                categoryOptions[param.Key] = (options = new HashSet<string>());
//              }
//              options.Add(sv.Value);
//            }
//          }
//        }
//      }

//      public IEnumerable<(string paramName, object value)> CreateCategories(IEnumerable<(string paramName, object value)> parameters) {
//        var categorizeableParams = parameters.Where(p => p.value is string);

//        foreach (var param in categorizeableParams) {
//          var options = CategoryOptions[param.paramName];
//          if (options.Count < 0) continue;
//          foreach (var option in options) {
//            string name = $"is{param.value}_{param.paramName}_Binary";
//            if ((string)param.value == option) {
//              yield return (name, 1);
//            } else {
//              yield return (name, 0);
//            }
//          }
//        }
//      }


//    }
//  }
//}