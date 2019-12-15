using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeuristicLab.RuntimePrediction.Persistence {
  class AnalyzeExperimentDocument {
    //public ObjectId Id { get; set; }
    public string Name { get; set; }

    [BsonId]
    public string FileName { get; set; }
    public string FullName { get; set; }
    public List<KeyValuePair<string,string>> GeneratedParameters { get; set; }
  }
}
