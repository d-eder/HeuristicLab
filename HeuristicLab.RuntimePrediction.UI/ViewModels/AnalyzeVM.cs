using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HeuristicLab.RuntimePrediction.Analyze;
using HeuristicLab.RuntimePrediction.Experiments;
using HeuristicLab.RuntimePrediction.UI.Common;

namespace HeuristicLab.RuntimePrediction.UI.ViewModels {
  class AnalyzeVM : ViewModel {
    private IExperimentCreator experimentCreator = ServiceFactory.GetExperimentCreator();
    private IParameterAnalyzerService analyzerService = ServiceFactory.GetParameterAnalyzerService();

    public ObservableCollection<ParameterItemVM> Parameters { get; } = new ObservableCollection<ParameterItemVM>();

    public ExperimentVM CurrentExperiment {
      get => currentExperiment; private set { currentExperiment = value; NotifyPropertyChanged(); }
    }

    public AnalyzeVM(Type algorithmType, Type problemType) {
      this.algorithmType = algorithmType;
      this.problemType = problemType;
    }

    private ICommand createExperimentsCommand;
    private ICommand loadExperimentsCommand;
    private Type algorithmType;
    private Type problemType;
    private ExperimentVM currentExperiment;

    public ICommand CreateExperimentsCommand { get => createExperimentsCommand ?? (createExperimentsCommand = new CommandHandler(() => CreateExperiments())); }
    public ICommand LoadExperimentsCommand { get => loadExperimentsCommand ?? (loadExperimentsCommand = new CommandHandler(() => LoadExperiments())); }

    public async void CreateExperiments() {
      var parameters = Parameters.Where(p => p.ParameterImpact.Impact == Analyze.Impact.Unknown);

      var experimentTask = experimentCreator.CreateExperimentForParameters(algorithmType, problemType, Parameters.Select(p => p.Parameter));

      var name = experimentCreator.GetExperimentName(algorithmType, problemType);
      var file = UIEventHandler.ShowSavedDialog(new SaveDialogEventArgs { FileName = name + ".hl" });
      if (file != null) {
        await DoSafe(async () => {
          var experiment = await experimentTask;
          await experimentCreator.SaveExperiment(experiment, file);
          await analyzerService.InsertAnalyzeExperiment(experiment);
          CurrentExperiment = new ExperimentVM(experiment);
          UIEventHandler.ShowMessage(
            new MessageEventArgs { Title = "Experiment saved", Content = $"Experiment saved in {file}, execute experiment and import results again" });
        });
      }
    }

    public async void LoadExperiments() {
      var file = UIEventHandler.ShowLoadDialog(new LoadDialogEventArgs());
      if (file != null) {
        await DoSafe(async () => {
          var experiment = await analyzerService.GetExperimentWithFilename(file);
          CurrentExperiment = new ExperimentVM(experiment);
        });
      }
    }
  }
}
