using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Optimization;

namespace HeuristicLab.RuntimePrediction {
  internal class HeuristicTypeFetcher : IHeuristicTypeFetcher {
    
    static HeuristicTypeFetcher() {
      LoadDlls();
    }

    public static void LoadDlls() {
        new DirectoryInfo("./")
          .GetFiles("*Algorithms*.dll")
          .ForEach(f => Assembly.LoadFile(f.FullName));

      new DirectoryInfo("./")
          .GetFiles("*Problems*.dll")
          .ForEach(f => Assembly.LoadFile(f.FullName));
    }


    public Task<IEnumerable<Type>> GetAlgorithmTypes() {
      return FindClassesWithInterface<IAlgorithm>(".Algorithms");
    }

    public Task<IEnumerable<Type>> GetProblemTypes() {
      return FindClassesWithInterface<IProblem>(".Problems");
    }

    private Task<IEnumerable<Type>> FindClassesWithInterface<TInterface>(string searchNs) {
      return Task.Run(() => {
        var interfaceType = typeof(TInterface);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        return from a in assemblies
               from t in a.GetTypes()
               where t.Namespace != null && t.Namespace.Contains(searchNs)
               where interfaceType.IsAssignableFrom(t)
               select t;
      });
    }
  }
}
