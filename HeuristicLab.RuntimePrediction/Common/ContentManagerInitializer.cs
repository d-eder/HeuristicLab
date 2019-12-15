using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Common;
using HeuristicLab.Core;

namespace HeuristicLab.RuntimePrediction.Common {
  static class ContentManagerInitializer {
    static ContentManagerInitializer() {
      ContentManager.Initialize(new PersistenceContentManager());
    }

    public static void Initialize() { }
  }
}
