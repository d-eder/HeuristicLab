using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Core;
using HeuristicLab.RuntimePrediction.Analyze;

namespace HeuristicLab.RuntimePrediction.UI.ViewModels {
  class ExperimentVM {
    private AnalyzeExperiment experiment;

    public List<KeyValuePair<string, string>> GeneratedParameters { get; private set; }

    public ExperimentVM(AnalyzeExperiment experiment) {
      this.experiment = experiment;
      this.GeneratedParameters = experiment.GeneratedParameters
                .GroupBy(p => p.parameterName, p => p.value)
                .Select(g => new KeyValuePair<string, string>(g.Key, string.Join(", ", g.ToArray())))
                .ToList();
    }
  }
}
