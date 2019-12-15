using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Algorithms.GeneticAlgorithm;
using HeuristicLab.Problems.TravelingSalesman;

namespace HeuristicLab.RuntimePrediction.UI.ViewModels {
  class MainVM : ViewModel, INavigateHandler {
    private ViewModel selectedVM;

    public AlgorithmSelectorVM AlgorithmSelectorVM { get; private set; }
    public ProblemSelectorVM ProblemSelectorVM { get; private set; }


    public MainVM() {
      AlgorithmSelectorVM = new AlgorithmSelectorVM(this);
      ProblemSelectorVM = new ProblemSelectorVM(this);
#if DEBUG
      AlgorithmSelectorVM.SelectedType = typeof(GeneticAlgorithm);
      ProblemSelectorVM.SelectedType = typeof(TravelingSalesmanProblem);
      SelectedVM = new ParameterVM(AlgorithmSelectorVM.SelectedType, ProblemSelectorVM.SelectedType); ;
#else
      SelectedVM = AlgorithmSelectorVM;
#endif
    }

    public ViewModel SelectedVM {
      get => selectedVM; set { selectedVM = value; NotifyPropertyChanged(); }
    }

    public void HandleNext(ViewModel current) {
      if (current is AlgorithmSelectorVM) {
        SelectedVM = ProblemSelectorVM;
      } else if (current is ProblemSelectorVM) {
        SelectedVM = new ParameterVM(AlgorithmSelectorVM.SelectedType, ProblemSelectorVM.SelectedType);
      } else {
        throw new Exception("invalid viewmodel");
      }
    }
  }
}
