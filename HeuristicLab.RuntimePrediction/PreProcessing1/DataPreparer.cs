//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using HeuristicLab.Analysis;
//using HeuristicLab.Core;
//using HeuristicLab.Optimization;
//using HeuristicLab.RuntimePrediction;
//using HeuristicLab.RuntimePrediction.PreProcessing;

//namespace HeuristicLab.RuntimePrediction {
//  using Kvp = KeyValuePair<string, object>;
//  using Parameters = IEnumerable<KeyValuePair<string, IItem>>;


//  internal partial class DataPreparer {


//    private readonly CategoryHandler categoryHandler = new CategoryHandler();

//    private void PrepareCreation(RunCollection runCollection) {
//      categoryHandler.ExtractCategories(runCollection);
//    }

//    public Data PrepareData(RunCollection runCollection) => PrepareData(new Data(), runCollection);

//    public Data PrepareData(Data data, RunCollection runCollection) {
//      PrepareCreation(runCollection);
//      AddDataFromRunCollection(data, runCollection);
//      return data;
//    }

//    public Data PrepareData(Parameters parameters) {
//      var data = new Data();
//      AddDataFromParameters(new Data(), parameters);
//      return data;
//    }
//    private void AddDataFromRunCollection(Data data, RunCollection runCollection) {
//      runCollection.ForEach(r =>
//      AddDataFromParameters(data, r.Results.Concat(r.Parameters).Distinct())
//      );
//    }

//    public Data CleanUpData(Data data) {
//      data.SetNullValues(p => p.EndsWith("Binary"), 0);
//      data.RemoveDistinctHeaders();
//      data.NormalizeNumericValues();
//      return data;
//    }

//    private ISet<string> ignoreParams = new HashSet<string>() {
//      "Algorithm Name","Seed","SetSeedRandomly","EvaluatedSolutions","CurrentBestQuality","CurrentAverageQuality","CurrentWorstQuality","AbsoluteDifferenceBestKnownToBest","RelativeDifferenceBestKnownToBest", "BestKnownQuality"
//    };


//    private void AddDataFromParameters(Data data, Parameters parameters) {
//      var relevantParameters = parameters.Where(p => !ignoreParams.Contains(p.Key) && !p.Key.Contains("Analyzer"));

//      var processedParameters = relevantParameters
//        .Where(kvp => kvp.Value != null)
//        .Select(kvp => ProcessParameter(kvp.Key, kvp.Value))
//        .SelectMany(list => list)
//        .ToList()
//      ;

//      processedParameters.AddRange(categoryHandler.CreateCategories(processedParameters).ToList());
//      processedParameters.AddRange(FeatureExtractor.ExtractProblemFeatures(parameters));

//      data.Add(processedParameters.Select(p => new Kvp(p.paramName, p.value)));
//    }

//    private IEnumerable<(string paramName, object value)> ProcessParameter(string paramName, IItem valItem) {
//      object value = GetItemValue(valItem);

//      if (value == null) {
//        //if (valItem is DataTable dt) {
//        //  yield return (paramName + "_DataTableRows", dt.Rows.Count);
//        //} else if (valItem is DoubleMatrix dm) {
//        //  yield return (paramName + "_MatrixRows", dm.Rows);
//        //  yield return (paramName + "_MatrixColumns", dm.Columns);
//        //} else if (valItem is IntMatrix im) {
//        //  yield return (paramName + "_MatrixRows", im.Rows);
//        //  yield return (paramName + "_MatrixColumns", im.Columns);
//        //} else if (valItem is DoubleArray da) {
//        //  yield return (paramName + "_ArrayLength", da.Length);
//        //} else if (valItem is IntArray ia) {
//        //  yield return (paramName + "_ArrayLength", ia.Length);
//        //} else {
//        //  Logger.Info("couldn't process type " + valItem.GetType());
//        //}

//      } else {
//        if (value is string strVal) {

//        } else if (value is TimeSpan ts) {
//          paramName += "_seconds";
//          value = ts.TotalSeconds;
//        }

//        yield return (paramName, value);
//      }
//    }


//    private IDictionary<Type, PropertyInfo> valueProperties = new Dictionary<Type, PropertyInfo>();

//    private object GetItemValue(IItem valueItem) {
//      if (valueItem == null) {
//        return null;
//      }

//      var type = valueItem.GetType();
//      if (!valueProperties.TryGetValue(type, out PropertyInfo valueProp)) {
//        valueProp = type.GetProperty("Value");
//        if (valueProp == null) {
//          return null;
//        }
//        valueProperties[type] = valueProp;
//      }

//      return valueProp.GetValue(valueItem);
//    }
//  }
//}