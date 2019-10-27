using System;
using HeuristicLab.Core;

namespace HeuristicLab.RuntimePrediction.Preprocessing {
  class Parameter {
    private ParameterValue value;

    public Parameter(string name, IItem item, object value) {
      Name = name;
      this.value = value == null ? ParameterValue.Empty : new ParameterValue(value);
      IsCategory = false;
      Item = item;
    }

    public Parameter Copy(object value) {
      var param = new Parameter(Name, Item, value);
      param.IsCategory = IsCategory;
      return param;
    }

    public string Name { get; private set; }
    public object Value { get => value.Value; set => this.value = new ParameterValue(value); }

    public ParameterValue ParamValue => value;

    public bool IsCategory { get; set; }
    public IItem Item { get; private set; }

    public bool HasValue => Value != null && !value.Equals(ParameterValue.Empty);

    public override string ToString() {
      return Name + "(" + Value + ")";
    }
  }

  struct ParameterValue {
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

