using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Algorithms.GeneticAlgorithm;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.PluginInfrastructure;
using HeuristicLab.RuntimePrediction.Analyze;
using HeuristicLab.RuntimePrediction.Common;
using HeuristicLab.RuntimePrediction.Preprocessing;
using Parameter = HeuristicLab.RuntimePrediction.Preprocessing.Parameter;

namespace HeuristicLab.RuntimePrediction.Experiments {
  class ExperimentCreator : IExperimentCreator {

    public Task SaveExperiment(AnalyzeExperiment experiment, string filename) {
      experiment.File = new FileInfo(filename);
      return Task.Run(() => {
        ContentManagerInitializer.Initialize();
        ContentManager.Save(experiment.Experiment, filename, true);
      });
    }

    public string GetExperimentName(Type algorithmType, Type problemType) {
      return $"ParameterExperiment_{algorithmType.Name}_{problemType.Name}";
    }

    public Task<AnalyzeExperiment> CreateExperimentForParameters(Type algorithmType, Type problemType, IEnumerable<Parameter> parameters) {
      return Task.Run(() => {
        var generatedParameters = new List<IParameter>();
        var optimizers = new List<IOptimizer>();
        foreach (var para in parameters) {
          foreach (var optimizer in CreateOptimizerForParameter(algorithmType, problemType, para)) {
            optimizers.Add(optimizer.algorithm);
            generatedParameters.Add(optimizer.parameter);
          }
        }

        var experiment = new Experiment(GetExperimentName(algorithmType, problemType), "Experiment to get runtime prediction");
        optimizers.ForEach(o => experiment.Optimizers.Add(o));

        var mappedParameters = generatedParameters.Select(p => (p.Name, p.ActualValue.GetUnderlyingValue().ToString()));
        return new AnalyzeExperiment { Experiment = experiment, GeneratedParameters = mappedParameters.ToList() };
      });
    }

    private IEnumerable<(IAlgorithm algorithm, IParameter parameter)> CreateOptimizerForParameter(Type algorithmType, Type problemType, Parameter parameter) {
      if (parameter.Item == null || parameter.Item.IsType(typeof(FixedValueParameter<>))) {
        yield break;
      }

      string paramName = parameter.Name;
      var list = new List<IAlgorithm>();

      foreach (var value in GenerateParameterValues(parameter)) {

        var algorithm = (IAlgorithm)Activator.CreateInstance(algorithmType);
        var problem = (IProblem)Activator.CreateInstance(problemType);

        algorithm.Problem = problem;

        IParameter algParam;
        if (!algorithm.Parameters.TryGetValue(paramName, out algParam) && !problem.Parameters.TryGetValue(paramName, out algParam)) {
          throw new ArgumentException($"could not find {parameter.Name} in algorithm {algorithmType} nor in problem {problemType}");
        }

        IItem v = value.GetValue(algParam);
        if (v != null) {
          algParam.ActualValue = v;
          yield return (algorithm, algParam);
        }
      }
    }

    class GeneratedValue {

      private object value;
      private bool fromValidValues = false;

      public GeneratedValue(bool value) {
        this.value = new BoolValue(value);
      }
      public GeneratedValue(int value) {
        this.value = new IntValue(value);
      }
      public GeneratedValue(PercentValue value) {
        this.value = value;
      }

      public GeneratedValue(Type value) {
        this.value = value;
        fromValidValues = true;
      }

      public IItem GetValue(IItem parameter) {
        if (!fromValidValues) {
          return (IItem)value;
        }

        var validValues = GetValidValues(parameter);
        return (IItem)validValues.Where(v => v.GetType() == (Type)value).SingleOrDefault();
      }
    }

    private static IEnumerable<object> GetValidValues(IItem item) {
      var propName = nameof(OptionalConstrainedValueParameter<BoolValue>.ValidValues);
      var prop = item.GetType().GetProperty(propName);
      var validValues = (IEnumerable<IItem>)prop.GetValue(item);

      if(validValues.Count() == 0) {
        var type = item.GetType().GetGenericArguments()[0];
        return ApplicationManager.Manager.GetInstances(type);
      }

      return validValues;
    }


    private IEnumerable<GeneratedValue> GenerateParameterValues(Parameter parameter) {
      var type = parameter.ParamValue.ValueType;
      var currentValue = parameter.ParamValue;
      var item = parameter.Item;

      if (type == typeof(BoolValue)) {
        yield return new GeneratedValue(true);
        yield return new GeneratedValue(false);
      } else if (type == typeof(IntValue)) {
        var intVal = ((IntValue)currentValue.Value).Value;
        if (intVal == 0) {
          for (int i = 0; i < 10; i++) {
            yield return new GeneratedValue(i);
          }
        } else {
          for (int i = 1; i <= 10; i++) {
            yield return new GeneratedValue(i * intVal);
            yield return new GeneratedValue(intVal / 5 * i);
          }
        }
      } else if (type == typeof(PercentValue)) {
        for (double d = 0.0; d <= 1; d += 0.1) {
          yield return new GeneratedValue(new PercentValue(d));
        }

      } else if (item.IsType(typeof(ConstrainedValueParameter<>))) {
        var validValues = GetValidValues(item);

        foreach (var value in validValues) {
          yield return new GeneratedValue(value.GetType());
        }
      }else if (item.IsType(typeof(ValueParameter<>))) {



      } else {
        // invalid
      }
    }
  }
}