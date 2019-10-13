using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Parameters;

namespace HeuristicLab.RuntimePrediction.Parameters {

  interface IParameterConfig {
    Type InstanceType { get; }

    void Apply(IParameterizedItem item);

    ISet<string> IgnoreParams { get; }
  }

  abstract class ParameterConfig<T> : IParameterConfig where T : IParameterizedItem {

    private readonly static LockedRandom random = new LockedRandom();
    private readonly List<Action<T>> setParamFunctions = new List<Action<T>>();

    protected void SetDefault() {
      setParamFunctions.Add(instance => {
        foreach (var param in instance.Parameters.Where(p => !IgnoreParams.Contains(p.Name))) {
          SetRandomValue(param);
        }
      });
    }

    private static void SetRandomValue(IParameter param) {
      switch (param.ActualValue) {
        case ValueTypeValue<bool> boolVal:
          boolVal.Value = random.NextBool();
          break;
        case ValueTypeValue<int> intVal:
          intVal.Value = random.Next(0, 1_000);
          break;
        case PercentValue percentValue:
          percentValue.Value = random.NextDouble();
          break;
        default: {
            var validValuesProp = param.GetType().GetProperty(nameof(IConstrainedValueParameter<IItem>.ValidValues));
            if (validValuesProp != null) { 
              var validValues = (IEnumerable<IItem>)validValuesProp.GetValue(param);
              param.ActualValue = random.FromCollection(validValues.ToList());
            } else {
              throw new ArgumentException($"could not set random param for type {param.GetType()}");
            }
            break;
          }
      }
    }

    protected void SetRangedParameter(Expression<Func<T, ValueTypeValue<int>>> expression, int from, int to) {
      var func = expression.Compile();
      setParamFunctions.Add(instance => func(instance).Value = random.Next(from, to));
    }

    public void Apply(IParameterizedItem item) {
      setParamFunctions.ForEach(a => a((T)item));
    }

    public Type InstanceType => typeof(T);

    public abstract ISet<string> IgnoreParams { get; }
  }
}

class LockedRandom {
  private Random random = new Random();

  

  internal int Next(int from, int to) {
    lock (this) {
      return random.Next(from, to);
    }
  }

  internal bool NextBool() {
    return NextDouble() >= 0.5;
  }

  internal double NextDouble() {
    lock (this) {
      return random.NextDouble();
    }
  }

  public T FromCollection<T>(ICollection<T> values) {
    return values.ElementAt(Next(0, values.Count));
  }
}
