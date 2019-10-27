using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.RuntimePrediction.Preprocessing {
  class Data {
    private SortedSet<string> headers = new SortedSet<string>();
    public IReadOnlyCollection<string> Headers => headers;

    public List<IDictionary<string, Parameter>> entries = new List<IDictionary<string, Parameter>>();

    private bool dirtyEntries = false;


    public void RemoveHeader(string header) {
      headers.Remove(header);
      entries.ForEach(e => e.Remove(header));
      dirtyEntries = true;
    }

    public void AddEntry(IEnumerable<Parameter> entry) { 
      entry.ForEach(e => headers.Add(e.Name));
      entries.Add(entry.ToDictionary(e => e.Name));
      dirtyEntries = true;
    }


    public IEnumerable<Parameter> GetParameters(string header) {
      CheckEntries();
      return entries.Select(e => e[header]);
    }

    public IEnumerable<object> GetValues(string header) => GetParameters(header).Select(p => p.Value);

    public List<IDictionary<string, Parameter>> GetEntries() {
      CheckEntries();
      return entries;
    }

    private void CheckEntries() {
      if (!dirtyEntries) return;
      foreach (var header in headers) {
        var baseParam = entries.Where(e => e.ContainsKey(header)).First()[header];
        foreach (var entry in entries) {
          if (!entry.ContainsKey(header)) {
            entry.Add(header, baseParam.Copy(null));
          }
        }
      }
    }
  }
}