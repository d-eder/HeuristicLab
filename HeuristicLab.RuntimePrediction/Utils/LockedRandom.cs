﻿using System;
using System.Collections.Generic;
using SRandom = System.Random;

namespace HeuristicLab.RuntimePrediction {
  
  class LockedRandom {
    private SRandom random = new SRandom();

    internal int Next(int from, int to) {
      lock (this) {
        return random.Next(from, to);
      }
    }

    internal bool NextBool() {
      return NextDouble() >= 0.5;
    }

    internal double NextDouble() {
      lock (this) {
        return random.NextDouble();
      }
    }

    internal T FromCollection<T>(ICollection<T> values) {
      lock (this) {
        return random.FromCollection(values);
      }
    }
  }
}
