using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.RuntimePrediction.UI.Common;

namespace HeuristicLab.RuntimePrediction.UI.ViewModels {
  class ViewModel : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    protected void NotifyPropertyChanged([CallerMemberName]string propertyName = "") {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task DoSafe(Func<Task> action) {
      try {
        await action();
      } catch (Exception e) {
        UIEventHandler.ShowMessage(new MessageEventArgs { Content = e.ToString(), Title = "Error" });
        Console.WriteLine(e);
      }
    }
  }
}
