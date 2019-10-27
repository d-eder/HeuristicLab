using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Optimization;

namespace HeuristicLab.RuntimePrediction.Preprocessing {

  class DataPreprocessor {

    private const string PROBLEM_NAME_PARAM = "Problem Name";
    private const string ALGORITHM_NAME_PARAM = "Algorithm Name";

    private static readonly IReadOnlyCollection<string> IncludeNames = new HashSet<string> { PROBLEM_NAME_PARAM, ALGORITHM_NAME_PARAM };

    private Data data = new Data();

    public Data Data => data;

    private string targetLabel;
    private List<RunCollection> runs = new List<RunCollection>();

    private CategoryHandler categoryHandler;

    public DataPreprocessor(string targetLabel) {
      this.targetLabel = targetLabel;
    }

    public DataPreprocessor(string targetLabel, RunCollection runs) : this(targetLabel) {
      this.runs.Add(runs);
    }

    public void AddRuns(RunCollection runs) {
      this.runs.Add(runs);
    }

    private IEnumerable<IRun> AllRuns => runs.SelectMany(r => r);

    // Parameter extrahieren
    // Parameter filtern
    // Parameter categorisieren
    // Features extrahieren
    // Cleanup - NullValues,DistinctHeaders,Normalize

    public void Process() {
      var allRuns = AllRuns.ToList();
      categoryHandler = new CategoryHandler(allRuns);

      foreach (var run in allRuns) {
        var parameters = new List<KeyValuePair<string, IItem>>(run.Parameters);
        if (!run.Results.ContainsKey(targetLabel))
          throw new ArgumentException("could not find targetLabel " + targetLabel);

        parameters.Add(new KeyValuePair<string, IItem>(targetLabel, run.Results[targetLabel]));
        ProcessParameters(parameters.Distinct());
      }

      ProcessData();
    }

    private ISet<string> ignoreParams = new HashSet<string> { "Seed" };
    private void ProcessParameters(IEnumerable<KeyValuePair<string, IItem>> parameters) {
      var convertedParameters = ConvertParameters(
        parameters.Where(p => !ignoreParams.Contains(p.Key) && !p.Key.Contains("."))
        
        );
      ProcessParameters(convertedParameters);
    }

    private IEnumerable<Parameter> ConvertParameters(IEnumerable<KeyValuePair<string, IItem>> parameters) {
      foreach (var p in parameters) {
        yield return new Parameter(p.Key, p.Value, ConvertItemToValue(p.Value));
      }
    }

    private object ConvertItemToValue(IItem item) {
      return ItemConverter.GetItemValue(item);
    }

    private void ProcessParameters(IEnumerable<Parameter> parameters) {
      var validParams = new List<Parameter>();
      foreach (var p in parameters) {
        ConvertInvalidParamValue(p);
        if (IsCategoryParameter(p)) {
          validParams.AddRange(categoryHandler.CreateCategories(p));
        }
        validParams.Add(p);
      }

      validParams.AddRange(FeatureExtractor.ExtractProblemFeatures(validParams.ToList()));
      data.AddEntry(validParams);
    }


    private bool IsCategoryParameter(Parameter p) {
      return p.ParamValue.ValueType == typeof(string) && p.Name != ALGORITHM_NAME_PARAM && p.Name != PROBLEM_NAME_PARAM;
    }

    private void ConvertInvalidParamValue(Parameter parameter) {
      if (parameter.Value is TimeSpan ts) {
        parameter.Value = ts.TotalSeconds;
      }
    }

    private void ProcessData() {
      CleanDistinctData();
      SetNullValues();
      NormalizeData();
    }

    private void SetNullValues() {
      foreach(var entry in data.GetEntries().SelectMany(d => d.Values)) {
        if(entry.IsCategory && entry.HasValue == false) {
          entry.Value = 0;
        }
      }
    }

    private void CleanDistinctData() {
      foreach (var header in new List<string>(data.Headers.Where(h => !IncludeNames.Contains(h)))) {
        var values = data.GetValues(header).ToHashSet();
        if (values.Count <= 1) {
          data.RemoveHeader(header);
        }
      }
    }

    private void NormalizeData() {
      foreach (var header in data.Headers.Where(h => h != targetLabel)) {
        var parameter = data.GetParameters(header).First();
        if (parameter.IsCategory) {
          continue;
        }

        var valueType = parameter.ParamValue.ValueType;
        if (valueType == typeof(double) || valueType == typeof(int)) {

          var values = data.GetValues(header).Where(v => v != null).Select(v => Convert.ToDouble(v)).ToList();
          //var avg = values.Average();
          var max = values.Max();
          var min = values.Min();
          //var standardDeviation = values.StandardDeviation();

          foreach (var param in data.GetParameters(header)) {
            double value = Convert.ToDouble(param.Value);

            // Min Max
            param.Value = (value - min) / (max - min);


            // Standardization 
            //param.Value = (value - avg) / standardDeviation;
          }
        }
      }
    }

    internal IEnumerable<dynamic> GetDataAsDynamic() {
      var headers = data.Headers.OrderBy(h => h != targetLabel).ThenBy(h => h);
      foreach (var row in data.GetEntries()) {
        IDictionary<string, object> item = new ExpandoObject();
        foreach (var header in headers) {
          item[header] = row[header].Value;
        }
        yield return item;
      }
    }
  }
}