using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Algorithms.GeneticAlgorithm;

namespace HeuristicLab.RuntimePrediction {
  class GAConfig : ParameterConfig<GeneticAlgorithm> {

    public GAConfig() {
      SetDefault();
      SetRangedParameter(a => a.Elites, 0, 10);
      SetRangedParameter(a => a.MaximumGenerations, 100, 1000);
      SetRangedParameter(a => a.PopulationSize, 100, 1000);
    }

    public override ISet<string> IgnoreParams { get; } = new HashSet<string>() {
      "Seed","SetSeedRandomly", "Analyzer"
    };

  }
}
