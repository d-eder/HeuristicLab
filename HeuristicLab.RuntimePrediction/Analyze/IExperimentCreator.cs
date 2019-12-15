using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HeuristicLab.Optimization;
using HeuristicLab.RuntimePrediction.Analyze;
using HeuristicLab.RuntimePrediction.Preprocessing;

namespace HeuristicLab.RuntimePrediction.Experiments {
  public interface IExperimentCreator {
    Task SaveExperiment(AnalyzeExperiment experiment, string filename);
    Task<AnalyzeExperiment> CreateExperimentForParameters(Type algorithmType, Type problemType, IEnumerable<Parameter> parameters);
    string GetExperimentName(Type algorithmType, Type problemType);
    }
}