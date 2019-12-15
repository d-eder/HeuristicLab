using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HeuristicLab.RuntimePrediction.UI.ViewModels {
  abstract class TypeSelectorVM : ViewModel{

    protected INavigateHandler navigateHandler;

    protected IHeuristicTypeFetcher typeFetcher = ServiceFactory.GetHeuristicTypeFetcher();
    public ObservableCollection<Type> Types { get; } = new ObservableCollection<Type>();

    private ICommand nextComand;
    public ICommand NextCommand { get => nextComand ?? (nextComand = new CommandHandler(() => navigateHandler.HandleNext(this))); }

    public abstract string Title { get; }

    protected TypeSelectorVM(INavigateHandler navigateHandler) {
      this.navigateHandler = navigateHandler;
      Load();
    }

    public bool HasSelectedItem => selectedType != null;

    private Type selectedType = null;
    public Type SelectedType {
      get => selectedType;
      set {
        selectedType = value;
        NotifyPropertyChanged(nameof(HasSelectedItem));
        NotifyPropertyChanged();
      }
    }

    private async void Load() {
      Types.AddAll(await GetTypes());
    }

    protected abstract Task<IEnumerable<Type>> GetTypes();
  }

  class AlgorithmSelectorVM : TypeSelectorVM {
    public AlgorithmSelectorVM(INavigateHandler navigateHandler) : base(navigateHandler) {
    }

    public override string Title => "Select algorithm:";

    protected override Task<IEnumerable<Type>> GetTypes() {
      return typeFetcher.GetAlgorithmTypes();
    }
  }

  class ProblemSelectorVM : TypeSelectorVM {
    public ProblemSelectorVM(INavigateHandler navigateHandler) : base(navigateHandler) {
    }

    public override string Title => "Select problem:";
    protected override Task<IEnumerable<Type>> GetTypes() {
      return typeFetcher.GetProblemTypes();
    }
  }
}
