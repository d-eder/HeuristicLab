using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Core;
using HeuristicLab.Optimization;

namespace HeuristicLab.RuntimePrediction {
  static class Parameterizer {

    private readonly static Dictionary<Type, IParameterConfig> configs = new Dictionary<Type, IParameterConfig>();

    static Parameterizer() {
      var ns = typeof(Parameterizer).Namespace;
      typeof(Parameterizer).Assembly.GetTypes()
        .Where(t => t.Namespace == ns)
        .Where(t => typeof(IParameterConfig).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
        .ForEach(t => {
          var config = (IParameterConfig)Activator.CreateInstance(t);
          configs[config.InstanceType] = config;
        });
    }


    internal static void SetParameters(IParameterizedNamedItem item) {
      if (!configs.TryGetValue(item.GetType(), out IParameterConfig config)) {
        throw new InvalidOperationException($"could not find config for {item.GetType()}");
      }
      config.Apply(item);
    }
  }
}
