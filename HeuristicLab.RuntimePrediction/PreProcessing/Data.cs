using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.RuntimePrediction {
  class Data {
    private HeaderCollection headers = new HeaderCollection();
    private List<Row> rows = new List<Row>();


    public void Add(IEnumerable<KeyValuePair<string, object>> data) {
      var row = new Row() {
        rowData = new List<object>(headers.Count)
      };
      foreach (var dataPoint in data) {
        int idx = headers.GetHeaderIndex(dataPoint.Key);
        row.Set(idx, dataPoint.Value);
      }
      rows.Add(row);
    }

    private struct Row {
      public List<object> rowData;

      public void Set(int idx, object data) {
        rowData.FillWithNull(idx + 1);
        rowData[idx] = data;
      }
    }

    public List<string> Header => headers.ToList();
    public List<List<object>> Values => rows.Select(r => r.rowData.FillWithNull(headers.Count)).ToList();


    public (IEnumerable<string> header, IEnumerable<List<object>> values) Get() {
      return (Header, Values);
    }

    private class HeaderCollection : IEnumerable<string> {
      private IDictionary<string, int> headers = new Dictionary<string, int>();

      public int GetHeaderIndex(string header) {
        if (!headers.TryGetValue(header, out int idx)) {
          idx = headers.Count();
          headers[header] = idx;
        }
        return idx;
      }

      public int Count => headers.Count();

      public IEnumerator<string> GetEnumerator() {
        return headers.Keys.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator() {
        return this.GetEnumerator();
      }
    }
  }
}