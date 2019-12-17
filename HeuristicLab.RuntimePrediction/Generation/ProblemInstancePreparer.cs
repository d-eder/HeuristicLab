using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Optimization;
using HeuristicLab.Problems.Instances;

namespace HeuristicLab.RuntimePrediction {


  static class ProblemInstancePreparer {

    private static IDictionary<Type, IProblemInstancePreparer> handlers = new Dictionary<Type, IProblemInstancePreparer>();

    public static IProblemInstancePreparer Create(IProblem problem) {
      lock (handlers) {
        var problemType = problem.GetType();
        if (!handlers.TryGetValue(problemType, out IProblemInstancePreparer handler)) {
          var problemInstanceConsumerType = problemType
            .GetInterfaces()
            .Where(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IProblemInstanceConsumer<>))
            .Select(iface => iface.GetGenericArguments().SingleOrDefault())
            .SingleOrDefault();
          handler = (IProblemInstancePreparer)Activator.CreateInstance(typeof(ProblemInstancePreparer<>).MakeGenericType(problemInstanceConsumerType), problem);
          handlers[problemType] = handler;
        }
        return handler;
      }
    }
  }

  interface IProblemInstancePreparer {
    ICollection<ProblemInstanceWrapper> Instances { get; }
    void SetInstance(IProblem problem, ProblemInstanceWrapper instance);
  }

  class ProblemInstancePreparer<TData> : IProblemInstancePreparer where TData : class {

    IProblemInstanceConsumer<TData> consumer;
    private ICollection<IProblemInstanceProvider<TData>> problemInstanceProviders;


    private ICollection<ProblemInstanceWrapper> instances;
    public ICollection<ProblemInstanceWrapper> Instances {
      get {
        if (instances == null) {
          instances = GetProblemInstanceWrappers().ToList();
        }
        return instances;
      }
    }

    public ProblemInstancePreparer(IProblemInstanceConsumer<TData> consumer) {
      this.consumer = consumer;
      problemInstanceProviders = ProblemInstanceManager.GetProviders(consumer)
        .Select(p => (IProblemInstanceProvider<TData>)p)
        .ToList();
    }

    private IEnumerable<ProblemInstanceWrapper> GetProblemInstanceWrappers() {
      foreach (var provider in problemInstanceProviders) {
        foreach (var dataDescriptor in provider.GetDataDescriptors()) {
          yield return new ProblemInstanceWrapper(provider, dataDescriptor);
        }
      }
    }

    public void SetInstance(IProblem problem, ProblemInstanceWrapper instance) {
      if (instance.InstanceData == null) {
        var provider = (IProblemInstanceProvider<TData>)instance.Provider;
        var data = provider.LoadData(instance.DataDescriptor);
        instance.InstanceData = data;
      }

      ((IProblemInstanceConsumer<TData>)problem).Load((TData)instance.InstanceData);
    }
  }
  
  class ProblemInstanceWrapper {
    public ProblemInstanceWrapper(IProblemInstanceProvider provider, IDataDescriptor dataDescriptor) {
      this.Provider = provider;
      this.DataDescriptor = dataDescriptor;
    }

    public IProblemInstanceProvider Provider { get; private set; }
    public IDataDescriptor DataDescriptor { get; private set; }
    public object InstanceData { get; set; }
  }
}