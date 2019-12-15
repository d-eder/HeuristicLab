using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HeuristicLab.RuntimePrediction.Analyze;
using HeuristicLab.RuntimePrediction.Preprocessing;

namespace HeuristicLab.RuntimePrediction.UI.ViewModels {
  class ParameterItemVM {
    private Parameter parameter;
    public ParameterImpact ParameterImpact { get; } = new ParameterImpact { Impact = Impact.Unknown };

    public string Name => Parameter.Name;
    public string RuntimeImpact => "Unknown";

    public Parameter Parameter { get => parameter; private set => parameter = value; }

    public ParameterItemVM(Parameter p) {
      this.Parameter = p;
    }
  }
}
