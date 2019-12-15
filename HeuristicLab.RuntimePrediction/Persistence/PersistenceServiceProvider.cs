using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.RuntimePrediction.Analyze;
using MongoDB.Driver;

namespace HeuristicLab.RuntimePrediction.Persistence {


  class PersistenceServiceProvider {

    private readonly MongoClient client;
    private readonly IMongoDatabase analyzeExperimentsDb;

    private IMongoCollection<AnalyzeExperimentDocument> AnalyzeExperimentDocument => analyzeExperimentsDb.GetCollection<AnalyzeExperimentDocument>("AnalyzeExperimentDocument");

    public PersistenceServiceProvider() {
      client = new MongoClient("mongodb://localhost:27017");
      analyzeExperimentsDb = client.GetDatabase("runtime-prediction-experiments");
    }
    
    public IMongoCollection<T> GetCollection<T>(){
      return analyzeExperimentsDb.GetCollection<T>(typeof(T).Name);
    }
  }
}
