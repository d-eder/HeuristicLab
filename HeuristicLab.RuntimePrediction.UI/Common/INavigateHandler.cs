using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.RuntimePrediction.UI.ViewModels {
  interface INavigateHandler {
    void HandleNext(ViewModel current);
  }
}
