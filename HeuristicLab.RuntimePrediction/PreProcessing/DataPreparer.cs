using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Analysis;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;
using HeuristicLab.RuntimePrediction;

namespace HeuristicLab.RuntimePrediction {
  using Kvp = KeyValuePair<string, object>;

  internal class DataPreparer {


    private StringParameterCategorizer categorizer = new StringParameterCategorizer();

    public Data CreateDataFromRuns(RunCollection runCollections) => CreateDataFromRuns(new[] { runCollections });

    public Data CreateDataFromRuns(IEnumerable<RunCollection> runCollections) {
      var data = new Data();
      runCollections.ForEach(rc => AddDataFromRunCollection(data, rc));
      return data;
    }
    public void AddRuns(Data data, IEnumerable<RunCollection> runCollections) {
      lock (data) {
        runCollections.ForEach(rc => AddDataFromRunCollection(data, rc));
      }
    }

    private ISet<string> ignoreParams = new HashSet<string>() {
      "Algorithm Name"
    };

    private void AddDataFromRunCollection(Data data, RunCollection runCollection) {
      foreach (var run in runCollection) {
        var relevantParameters = run.Results.Concat(run.Parameters)
          .Where(p => !ignoreParams.Contains(p.Key) && !p.Key.Contains("Analyzer"))
          ;

        var parameters = relevantParameters
          .Where(kvp => kvp.Value != null)
          .Select(kvp => ProcessParameter(kvp.Key, kvp.Value))
          .SelectMany(list => list)
          .Select(p => new Kvp(p.paramName, p.value))
        ;

        data.Add(parameters);
      }
    }


    private IEnumerable<(string paramName, object value)> ProcessParameter(string paramName, IItem valItem) {
      object value = GetItemValue(valItem);

      if (value == null) {
        if (value is DataTable dt) {
          yield return (paramName + "_DataTableRows", dt.Rows.Count);
        } else if (value is DoubleMatrix dm) {
          yield return (paramName + "_MatrixRows", dm.Rows);
          yield return (paramName + "_MatrixColumns", dm.Columns);
        } else if (value is IntMatrix im) {
          yield return (paramName + "_MatrixRows", im.Rows);
          yield return (paramName + "_MatrixColumns", im.Columns);
        } else if (value is DoubleArray da) {
          yield return (paramName + "_ArrayLength", da.Length);
        } else if (value is IntArray ia) {
          yield return (paramName + "_ArrayLength", ia.Length);
        } else {
          Logger.Info("could process type " + valItem.GetType());
        }

      } else {
        if (value is string strVal) {
          int cat = categorizer.Categorize(paramName, strVal);
          yield return (paramName + "_categorized", cat);
        }
      }
    }

    private IDictionary<Type, PropertyInfo> valueProperties = new Dictionary<Type, PropertyInfo>();

    private object GetItemValue(IItem valueItem) {
      if (valueItem == null) {
        return null;
      }

      var type = valueItem.GetType();
      if (!valueProperties.TryGetValue(type, out PropertyInfo valueProp)) {
        valueProp = type.GetProperty("Value");
        if (valueProp == null) {
          return null;
        }
        valueProperties[type] = valueProp;
      }

      return valueProp.GetValue(valueItem);
    }
  }
}