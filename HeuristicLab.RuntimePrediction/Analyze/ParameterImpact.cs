using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.RuntimePrediction.Analyze {
  public class ParameterImpact {
    public Impact Impact {get;set;}
  }

  public enum Impact {
    High, Medium, Low, NoImpact, Unknown
  }
}
