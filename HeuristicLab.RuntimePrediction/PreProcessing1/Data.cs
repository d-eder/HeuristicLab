//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Dynamic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using HeuristicLab.Common;

//namespace HeuristicLab.RuntimePrediction {
//  class Data {
//    private HeaderCollection headers = new HeaderCollection();
//    private List<Row> rows = new List<Row>();


//    public void Add(IEnumerable<KeyValuePair<string, object>> data) {
//      var row = new Row() {
//        data = new List<object>(headers.Count)
//      };

//      foreach (var dataPoint in data) {
//        int idx = headers.GetHeaderIndex(dataPoint.Key);
//        row.Set(idx, dataPoint.Value);
//      }
//      rows.Add(row);
//    }

//    private void RemoveHeader(string name) {
//      headers.Remove(name);
//    }


//    internal void SetNullValues(Func<string, bool> paramPredicate, object value) {
//      foreach (var header in headers) {
//        var idx = headers.GetHeaderIndex(header);
//        if (paramPredicate(header)) {
//          foreach (var row in GetRows()) {
//            var rowValue = row[idx];
//            if (rowValue == null || (rowValue is string sVal && string.IsNullOrWhiteSpace(sVal))) {
//              row[idx] = value;
//            }
//          }
//        }
//      }
//    }

//    public void RemoveDistinctHeaders() {
//      foreach (var header in headers.ToList()) {
//        var items = Get(header).ToHashSet();
//        if (items.Count <= 1) {
//          RemoveHeader(header);
//        }
//      }
//    }




//    public void NormalizeNumericValues() {
//      var calcData = new List<(double avg, double standardDeviation, int idx)>();
//      foreach (var header in headers) {
//        var data = Get(header).First();

//        if (data is double || data is int || data is decimal || data is float) {
//          int headerIdx = headers.GetHeaderIndex(header);
//          var values = GetRows().Select(r => Convert.ToDouble(r[headerIdx])).ToList();
//          var standardDeviation = values.StandardDeviation();
//          var avg = values.Average();
//          calcData.Add((avg, standardDeviation, headerIdx));
//        }
//      }

//      foreach (var row in GetRows()) {
//        foreach (var d in calcData) {
//          // normalize
//          row[d.idx] = (Convert.ToDouble(row[d.idx]) - d.avg) / d.standardDeviation;
//        }
//      }
//    }

//    public IEnumerable<object> Get(string header) {
//      var idx = headers.GetHeaderIndex(header);
//      return GetRows().Select(r => r[idx]);
//    }
//    private IEnumerable<List<object>> GetRows() {
//      return rows.Select(r => r.data.FillWithNull(headers.Count));
//    }

//    private struct Row {
//      public List<object> data;

//      public void Set(int idx, object value) {
//        data.FillWithNull(idx + 1);
//        data[idx] = value;
//      }
//    }

//    public List<string> Header => headers.ToList();
//    public List<List<object>> Values => GetRows().ToList();


//    public (IEnumerable<string> header, IEnumerable<List<object>> values) Get() {
//      return (Header, Values);
//    }

//    private class HeaderCollection : IEnumerable<string> {
//      private readonly IDictionary<string, int> headers = new Dictionary<string, int>();

//      public int GetHeaderIndex(string header) {
//        if (!headers.TryGetValue(header, out int idx)) {
//          idx = headers.Count();
//          headers[header] = idx;
//        }
//        return idx;
//      }

//      public void Remove(string header) {
//        headers.Remove(header);
//      }

//      public int Count => headers.Count();

//      public IEnumerator<string> GetEnumerator() {
//        return headers.Keys.GetEnumerator();
//      }

//      IEnumerator IEnumerable.GetEnumerator() {
//        return this.GetEnumerator();
//      }
//    }

//    internal IEnumerable<dynamic> AsDynamic() {
//      foreach (var row in GetRows()) {
//        IDictionary<string, object> item = new ExpandoObject();
//        foreach (var h in Header) {
//          int idx = headers.GetHeaderIndex(h);
//          item[h] = row[idx];
//        }
//        yield return item;
//      }
//    }
//  }
//}