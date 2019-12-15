using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HeuristicLab.RuntimePrediction.UI.Common;
using Microsoft.Win32;

namespace HeuristicLab.RuntimePrediction.UI.Views {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
      UIEventHandler.OnShowSaveDialog += UIEventHandler_OnShowSaveDialog;
      UIEventHandler.OnShowMessage += UIEventHandler_OnShowMessage;
      UIEventHandler.OnShowLoadDialog += UIEventHandler_OnShowLoadDialog;
    }



    private void UIEventHandler_OnShowMessage(MessageEventArgs args) {
      MessageBox.Show(this, args.Content, args.Title);
    }

    private string UIEventHandler_OnShowSaveDialog(SaveDialogEventArgs args) {
      var dialog = new SaveFileDialog {
        Filter = "HL-Files|*.hl",
        DefaultExt = "hl",
        FileName = args.FileName
      };
      if (args.InitialDirectory != null)
        dialog.InitialDirectory = args.InitialDirectory;

      if (dialog.ShowDialog(this).GetValueOrDefault(false)) {
        return dialog.FileName;
      }
      return null;
    }

    private string UIEventHandler_OnShowLoadDialog(LoadDialogEventArgs arg) {
      var dialog = new OpenFileDialog {
        Filter = "HL-Files|*.hl",
        DefaultExt = "hl"
      };
      if (dialog.ShowDialog(this).GetValueOrDefault(false)) {
        return dialog.FileName;
      }
      return null;
    }
  }
}
