using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Common;
using HeuristicLab.Optimization;
using HeuristicLab.RuntimePrediction.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeuristicLab.RuntimePrediction.Analyze {

  public interface IParameterAnalyzerService {
    Task InsertAnalyzeExperiment(AnalyzeExperiment experiment);
    Task<AnalyzeExperiment> GetExperimentWithFilename(string fileName);
  }

  class ParameterAnalyzerService : IParameterAnalyzerService {

    private PersistenceServiceProvider provider;
    public ParameterAnalyzerService(PersistenceServiceProvider provider) {
      this.provider = provider;
    }

    public async Task InsertAnalyzeExperiment(AnalyzeExperiment experiment) {
      var col = provider.GetCollection<AnalyzeExperimentDocument>();

      var fileName = experiment.File.Name;
      await col.DeleteOneAsync(d => d.FileName == fileName);
      
      var document = new AnalyzeExperimentDocument {
        Name = experiment.Experiment.Name,
        FileName = experiment.File.Name,
        FullName = experiment.File.FullName,
        GeneratedParameters = experiment.GeneratedParameters.Select(t => new KeyValuePair<string,string>(t.parameterName, t.value)).ToList()
      };
      await col.InsertOneAsync(document);
    }

    public async Task<AnalyzeExperiment> GetExperimentWithFilename(string fileName) {
      var file = new FileInfo(fileName);
      var col = provider.GetCollection<AnalyzeExperimentDocument>();
      var doc = await (await col.FindAsync(a => a.FileName == file.Name)).SingleAsync();
      return new AnalyzeExperiment {
        File = file,
        Experiment = (Experiment)ContentManager.Load(fileName),
        GeneratedParameters = doc.GeneratedParameters.Select(k => (k.Key, k.Value)).ToList()
      };
    }
  }
}
