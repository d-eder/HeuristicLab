﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Common;
using HeuristicLab.Optimization;
using HeuristicLab.RuntimePrediction.Common;
using HeuristicLab.RuntimePrediction.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeuristicLab.RuntimePrediction.Analyze {

  public interface IParameterAnalyzerService {
    Task SaveExperiment(AnalyzeExperiment experiment);
    Task<AnalyzeExperiment> GetExperimentWithFilename(string fileName);
  }

  class ParameterAnalyzerService : IParameterAnalyzerService {

    private PersistenceServiceProvider provider;
    public ParameterAnalyzerService(PersistenceServiceProvider provider) {
      this.provider = provider;
      ContentManagerInitializer.Initialize();
    }

    public async Task SaveExperiment(AnalyzeExperiment experiment) {
      var col = provider.GetCollection<AnalyzeExperimentDocument>();

      ContentManager.Save(experiment.Experiment, experiment.File.FullName, true);

      var fileName = experiment.File.Name;
      await col.DeleteOneAsync(d => d.FileName == fileName);

      var document = new AnalyzeExperimentDocument {
        Name = experiment.Experiment.Name,
        FileName = experiment.File.Name,
        FullName = experiment.File.FullName,
        GeneratedParameters = experiment.GeneratedParameters.Select(t => new KeyValuePair<string, string>(t.parameterName, t.value)).ToList()
      };
      await col.InsertOneAsync(document);
    }

    public async Task<AnalyzeExperiment> GetExperimentWithFilename(string fileName) {
      var file = new FileInfo(fileName);
      var exp = (Experiment)ContentManager.Load(fileName);
      var col = provider.GetCollection<AnalyzeExperimentDocument>();
      var doc = await (await col.FindAsync(a => a.FileName == file.Name)).SingleOrDefaultAsync();
      if (doc == null) return null;
      return new AnalyzeExperiment {
        File = file,
        Experiment = exp,
        GeneratedParameters = doc.GeneratedParameters.Select(k => (k.Key, k.Value)).ToList()
      };
    }
  }
}
