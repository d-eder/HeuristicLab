using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace HeuristicLab.RuntimePrediction {
  public class CsvData {


  }

  public class CsvUtil {
    const string DELIMETER = "\t";

    internal static void Write(FileInfo outputFile, IEnumerable<object> data) {
      var records = new List<dynamic>();

      //for (int i = 0; i < values.Count; i++) {
      //  IDictionary<string, object> record = new ExpandoObject();
      //  for (int j = 0; j < header.Count; j++) {
      //    string h = header[j];
      //    object val = values[i][j];
      //    record.Add(h, val);
      //  }
      //  records.Add(record);
      //}

      using var csv = new CsvWriter(new StreamWriter(outputFile.FullName));
      csv.WriteRecords(data);

      //List<string> lines = new List<string>();
      //lines.Add(string.Join(DELIMETER, header));
      //values.ForEach(v => lines.Add(string.Join(DELIMETER, v)));
      //File.WriteAllLines(outputFile.FullName, lines);
    }
  }
}
