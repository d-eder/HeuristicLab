using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Core;

namespace HeuristicLab.RuntimePrediction.Preprocessing {
  static class ItemConverter {
    private static IDictionary<Type, PropertyInfo> valueProperties = new Dictionary<Type, PropertyInfo>();
    public  static object GetItemValue(IItem valueItem) {
      if (valueItem == null) {
        return null;
      }

      var type = valueItem.GetType();
      if (!valueProperties.TryGetValue(type, out PropertyInfo valueProp)) {
        valueProp = type.GetProperty("Value");
        if (valueProp == null) {
          return null;
        }
        valueProperties[type] = valueProp;
      }

      return valueProp.GetValue(valueItem);
    }
  }
}
