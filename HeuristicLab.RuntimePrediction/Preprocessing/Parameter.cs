using System;
using HeuristicLab.Core;

namespace HeuristicLab.RuntimePrediction.Preprocessing {

  public class CategoryParameter : Parameter {
    public Parameter BaseParameter { get; private set; }
    public CategoryParameter(Parameter baseParameter, string name,  object value) : base(name, baseParameter.Item, value) {
      BaseParameter = BaseParameter;
    }
  }

  public class Parameter {
    private ParameterValue value;



    public Parameter(string name, IItem item, object value) {
      Name = name;
      this.value = value == null ? ParameterValue.Empty : new ParameterValue(value);
      Item = item;
    }

    public Parameter Copy(object value, IItem item = null) {
      if (item == null) item = Item;

      if (IsCategory) {
        var cat = this as CategoryParameter;
        return new CategoryParameter(cat.BaseParameter, Name, value);
      } else {
        var param = new Parameter(Name, Item, value);
        return param;
      }
    }

    public string Name { get; private set; }
    public object Value { get => value.Value; set => this.value = new ParameterValue(value); }

    public ParameterValue ParamValue => value;

    public bool IsCategory => this is CategoryParameter;
    public IItem Item { get; private set; }

    public bool HasValue => Value != null && !value.Equals(ParameterValue.Empty);

    public override string ToString() {
      return Name + "(" + Value + ")";
    }
  }

  public struct ParameterValue {
    public ParameterValue(object value) {
      this.Value = value;
    }

    public readonly static ParameterValue Empty;

    public object Value { get; set; }
    public Type ValueType => Value?.GetType();

    public override string ToString() {
      return Equals(Empty) ? "Empty" : Value?.ToString();
    }
  }
}

