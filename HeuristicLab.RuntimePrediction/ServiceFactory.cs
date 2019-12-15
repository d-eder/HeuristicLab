using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.RuntimePrediction.Analyze;
using HeuristicLab.RuntimePrediction.Experiments;
using HeuristicLab.RuntimePrediction.Persistence;
using HeuristicLab.RuntimePrediction.Preprocessing;

namespace HeuristicLab.RuntimePrediction {
  public static class ServiceFactory {
    public static IHeuristicTypeFetcher GetHeuristicTypeFetcher() => new HeuristicTypeFetcher();
    public static IParameterProcessor GetParameterProcessor() => new ParameterProcessor(new CategoryHandler());
    public static IParameterFetcher GetParameterFetcher() => new ParameterFetcher();
    public static IExperimentCreator GetExperimentCreator() => new ExperimentCreator();

    private static Lazy<PersistenceServiceProvider> serviceProvider = new Lazy<PersistenceServiceProvider>(() => new PersistenceServiceProvider());
    public static IParameterAnalyzerService GetParameterAnalyzerService() => new ParameterAnalyzerService(serviceProvider.Value);
  }
}
