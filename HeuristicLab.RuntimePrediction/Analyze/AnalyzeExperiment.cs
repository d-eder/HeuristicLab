using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Core;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;

namespace HeuristicLab.RuntimePrediction.Analyze {
  public class AnalyzeExperiment {
    public Experiment Experiment { get; internal set; }
    public IReadOnlyList<(string parameterName, string value)> GeneratedParameters { get; internal set; }
    public FileInfo File { get; set; }
  }
}
