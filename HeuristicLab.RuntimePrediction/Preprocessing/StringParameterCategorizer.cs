using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.RuntimePrediction {
  class StringParameterCategorizer {

    private IDictionary<string, Category> categories = new Dictionary<string, Category>();

    internal int Categorize(string key, string catValue) {
      if (!categories.TryGetValue(key, out Category cat)) {
        cat = new Category();
        categories.Add(key, cat);
      }

      return cat.GetCategoryValue(catValue);
    }
  }

  class Category {
    private int nr = 1;

    private IDictionary<string, int> categoryValues = new Dictionary<string, int>();

    internal int GetCategoryValue(string catValue) {
      if (!categoryValues.TryGetValue(catValue, out int catNumber)) {
        catNumber = nr++;
        categoryValues[catValue] = catNumber;
      }

      return catNumber;
    }
  }
}
