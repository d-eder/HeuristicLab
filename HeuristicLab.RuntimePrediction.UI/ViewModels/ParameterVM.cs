using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HeuristicLab.Core;
using HeuristicLab.RuntimePrediction.Analyze;
using HeuristicLab.RuntimePrediction.Preprocessing;

namespace HeuristicLab.RuntimePrediction.UI.ViewModels {
  class ParameterVM : ViewModel {

    private IParameterFetcher fetcher = ServiceFactory.GetParameterFetcher();
    private IParameterProcessor processor = ServiceFactory.GetParameterProcessor();
    private ParameterVM selectedParameter;

    public ObservableCollection<ParameterItemVM> Parameters { get; } = new ObservableCollection<ParameterItemVM>();

    public string Name => AlgorithmName + " - " + ProblemName;

    public string AlgorithmName => AlgorithmType.Name;
    public string ProblemName => ProblemType.Name;

    public ParameterVM SelectedParameter { get => selectedParameter; set { selectedParameter = value; NotifyPropertyChanged(); } }


    public AnalyzeVM AnalyzeVM { get; private set; }



    public ParameterVM(Type algorithmType, Type problemType) {
      this.AlgorithmType = algorithmType;
      this.ProblemType = problemType;
      this.AnalyzeVM = new AnalyzeVM(AlgorithmType, ProblemType);
      Load();
    }

    public Type AlgorithmType { get; private set; }
    public Type ProblemType { get; private set; }

    public async void Load() {
      var p = await GetParameters();
      Parameters.AddAll(p);
      AnalyzeVM.Parameters.AddAll(p);
    }

    private Task<List<ParameterItemVM>> GetParameters() {
      return Task.Run(() => {
        var plainParams = fetcher.GetParameters(AlgorithmType, ProblemType);
        var processed = processor.ProcessParameters(plainParams);
        return processed.Select(p => new ParameterItemVM(p)).ToList();
      });
    }
  }
}
