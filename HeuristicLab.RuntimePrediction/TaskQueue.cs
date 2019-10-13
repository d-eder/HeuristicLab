using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeuristicLab.RuntimePrediction {
  class TaskQueue<T> where T : Task {

    private bool finishedFlag = false;
    public bool Finished => finishedFlag && generatingTasks.Count == 0 && finishedTasks.Count == 0;

    private Semaphore semaphore;
    private BlockingCollection<T> finishedTasks = new BlockingCollection<T>();
    private ICollection<T> generatingTasks = new HashSet<T>();

    public TaskQueue(int size) {
      semaphore = new Semaphore(size-1, size);
    }

    public void Add(T task) {
      semaphore.WaitOne();
      lock (generatingTasks) {
        generatingTasks.Add(task);
      }
      task.ContinueWith(_ => TaskFinished(task));
    }

    private void TaskFinished(T task) {
      semaphore.Release();
      finishedTasks.Add(task);

      lock (generatingTasks) {
        generatingTasks.Remove(task);
      }
    }

    public T GetTask() {
      return finishedTasks.Take();
    }

    public IEnumerable<T> GetTasksLazy() {
      while (!Finished) {
        yield return GetTask();
      }
    }

    public void SetFinished() {
      finishedFlag = true;
    }
  }
}
