using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.RuntimePrediction.UI.Common {
  static class UIEventHandler {

    public static event Func<SaveDialogEventArgs, string> OnShowSaveDialog;
    public static event Func<LoadDialogEventArgs, string> OnShowLoadDialog;
    public static event Action<MessageEventArgs> OnShowMessage;


    public static string ShowSavedDialog(SaveDialogEventArgs args) {
      return OnShowSaveDialog?.Invoke(args);
    }

    public static string ShowLoadDialog(LoadDialogEventArgs args) {
      return OnShowLoadDialog?.Invoke(args);
    }

    public static void ShowMessage(MessageEventArgs msg) {
      OnShowMessage?.Invoke(msg);
    }
  }

  class SaveDialogEventArgs { 
    public string FileName{ get; set; }
    public string InitialDirectory { get; set; }
  }

  class LoadDialogEventArgs {
  }


  class MessageEventArgs {
    public string Title{ get; set; }
    public string Content { get; set; }
  }
}
