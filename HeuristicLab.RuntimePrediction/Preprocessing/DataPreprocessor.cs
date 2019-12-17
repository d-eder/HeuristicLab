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



    private static readonly IReadOnlyCollection<string> IncludeNames = new HashSet<string> { ParameterProcessor.PROBLEM_NAME_PARAM, ParameterProcessor.ALGORITHM_NAME_PARAM };

    private ParameterProcessor parameterProcessor;

    private Data data = new Data();

    public Data Data => data;

    private string targetLabel;
    private List<RunCollection> runs = new List<RunCollection>();

    public IReadOnlyCollection<RunCollection> RunCollections => runs;

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
      parameterProcessor = new ParameterProcessor(categoryHandler);

      foreach (var run in allRuns) {
        var parameters = new List<KeyValuePair<string, IItem>>(run.Parameters);
        if (!run.Results.ContainsKey(targetLabel))
          throw new ArgumentException("could not find targetLabel " + targetLabel);

        parameters.Add(new KeyValuePair<string, IItem>(targetLabel, run.Results[targetLabel]));
        var processed = parameterProcessor.ProcessParameters(parameters.Distinct());
        data.AddEntry(processed);
      }

      ProcessData();
    }

    private void ProcessData() {
      SetNullValues();
      NormalizeData();
      CleanDistinctData();
    }

    private void SetNullValues() {
      foreach (var entry in data.GetEntries().SelectMany(d => d.Values)) {
        if (entry.IsCategory && entry.HasValue == false) {
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
          
          var max = values.Max();
          var min = values.Min();

          //var avg = values.Average();
          //var standardDeviation = values.StandardDeviation();

          foreach (var param in data.GetParameters(header)) {
            double value = Convert.ToDouble(param.Value);

            // Min Max
            param.Value = (value - min) / (max - min);

            // Z-Score
            //param.Value = (value - avg) / standardDeviation;;
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