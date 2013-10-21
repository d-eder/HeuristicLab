﻿#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2013 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
 *
 * This file is part of HeuristicLab.
 *
 * HeuristicLab is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * HeuristicLab is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with HeuristicLab. If not, see <http://www.gnu.org/licenses/>.
 */
#endregion

using System;
using System.Linq;
using HeuristicLab.Analysis;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.Algorithms.SimulatedAnnealing {
  /// <summary>
  /// A simulated annealing improvement operator.
  /// </summary>
  [Item("SimulatedAnnealingImprovementOperator", "A simulated annealing improvement operator.")]
  [StorableClass]
  public sealed class SimulatedAnnealingImprovementOperator : SingleSuccessorOperator, ILocalImprovementOperator, IStochasticOperator {
    #region IGenericLocalImprovementOperator Properties
    public Type ProblemType { get { return typeof(ISingleObjectiveHeuristicOptimizationProblem); } }
    public IProblem Problem {
      get { return problem; }
      set {
        if (problem != value) {
          if (value != null && !(value is ISingleObjectiveHeuristicOptimizationProblem))
            throw new ArgumentException("Only problems of type " + ProblemType.ToString() + " can be assigned.");
          if (problem != null) DeregisterProblemEventHandlers();
          problem = (ISingleObjectiveHeuristicOptimizationProblem)value;
          if (problem != null) RegisterProblemEventHandlers();
          UpdateProblem();
        }
      }
    }
    #endregion

    [Storable]
    private ISingleObjectiveHeuristicOptimizationProblem problem;
    [Storable]
    private SimulatedAnnealingMainLoop loop;
    [Storable]
    private BestAverageWorstQualityAnalyzer qualityAnalyzer;

    #region Parameter Properties
    public ILookupParameter<IRandom> RandomParameter {
      get { return (ILookupParameter<IRandom>)Parameters["Random"]; }
    }
    public IConstrainedValueParameter<IMoveGenerator> MoveGeneratorParameter {
      get { return (IConstrainedValueParameter<IMoveGenerator>)Parameters["MoveGenerator"]; }
    }
    public IConstrainedValueParameter<IMoveMaker> MoveMakerParameter {
      get { return (IConstrainedValueParameter<IMoveMaker>)Parameters["MoveMaker"]; }
    }
    public IConstrainedValueParameter<ISingleObjectiveMoveEvaluator> MoveEvaluatorParameter {
      get { return (IConstrainedValueParameter<ISingleObjectiveMoveEvaluator>)Parameters["MoveEvaluator"]; }
    }
    private IValueLookupParameter<IntValue> InnerIterationsParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters["InnerIterations"]; }
    }
    public ValueParameter<MultiAnalyzer> AnalyzerParameter {
      get { return (ValueParameter<MultiAnalyzer>)Parameters["Analyzer"]; }
    }
    private ValueParameter<DoubleValue> StartTemperatureParameter {
      get { return (ValueParameter<DoubleValue>)Parameters["StartTemperature"]; }
    }
    private ValueParameter<DoubleValue> EndTemperatureParameter {
      get { return (ValueParameter<DoubleValue>)Parameters["EndTemperature"]; }
    }
    public IConstrainedValueParameter<IDiscreteDoubleValueModifier> AnnealingOperatorParameter {
      get { return (IConstrainedValueParameter<IDiscreteDoubleValueModifier>)Parameters["AnnealingOperator"]; }
    }
    public ScopeTreeLookupParameter<DoubleValue> QualityParameter {
      get { return (ScopeTreeLookupParameter<DoubleValue>)Parameters["Quality"]; }
    }
    #region ILocalImprovementOperator Parameters
    public IValueLookupParameter<IntValue> MaximumIterationsParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters["MaximumIterations"]; }
    }
    public ILookupParameter<IntValue> EvaluatedSolutionsParameter {
      get { return (ILookupParameter<IntValue>)Parameters["EvaluatedSolutions"]; }
    }
    public ILookupParameter<ResultCollection> ResultsParameter {
      get { return (ILookupParameter<ResultCollection>)Parameters["Results"]; }
    }
    #endregion
    #endregion

    #region Properties
    public IMoveGenerator MoveGenerator {
      get { return MoveGeneratorParameter.Value; }
      set { MoveGeneratorParameter.Value = value; }
    }
    public IMoveMaker MoveMaker {
      get { return MoveMakerParameter.Value; }
      set { MoveMakerParameter.Value = value; }
    }
    public IDiscreteDoubleValueModifier AnnealingOperator {
      get { return AnnealingOperatorParameter.Value; }
      set { AnnealingOperatorParameter.Value = value; }
    }
    public ISingleObjectiveMoveEvaluator MoveEvaluator {
      get { return MoveEvaluatorParameter.Value; }
      set { MoveEvaluatorParameter.Value = value; }
    }
    public MultiAnalyzer Analyzer {
      get { return AnalyzerParameter.Value; }
      set { AnalyzerParameter.Value = value; }
    }
    #endregion

    [StorableConstructor]
    private SimulatedAnnealingImprovementOperator(bool deserializing) : base(deserializing) { }
    private SimulatedAnnealingImprovementOperator(SimulatedAnnealingImprovementOperator original, Cloner cloner)
      : base(original, cloner) {
      this.problem = cloner.Clone(original.problem);
      this.loop = cloner.Clone(original.loop);
      this.qualityAnalyzer = cloner.Clone(original.qualityAnalyzer);
      RegisterEventHandlers();
    }
    public SimulatedAnnealingImprovementOperator()
      : base() {
      loop = new SimulatedAnnealingMainLoop();

      qualityAnalyzer = new BestAverageWorstQualityAnalyzer();

      Parameters.Add(new LookupParameter<IRandom>("Random", "The random number generator to use."));
      Parameters.Add(new ConstrainedValueParameter<IMoveGenerator>("MoveGenerator", "The operator used to generate moves to the neighborhood of the current solution."));
      Parameters.Add(new ConstrainedValueParameter<IMoveMaker>("MoveMaker", "The operator used to perform a move."));
      Parameters.Add(new ConstrainedValueParameter<ISingleObjectiveMoveEvaluator>("MoveEvaluator", "The operator used to evaluate a move."));
      Parameters.Add(new ValueLookupParameter<IntValue>("MaximumIterations", "The maximum number of generations which should be processed.", new IntValue(150)));
      Parameters.Add(new ValueLookupParameter<IntValue>("InnerIterations", "Number of moves that MultiMoveGenerators should create. This is ignored for Exhaustive- and SingleMoveGenerators.", new IntValue(1500)));
      Parameters.Add(new LookupParameter<IntValue>("EvaluatedSolutions", "The number of evaluated moves."));
      Parameters.Add(new ValueParameter<MultiAnalyzer>("Analyzer", "The operator used to analyze the solution.", new MultiAnalyzer()));
      Parameters.Add(new ValueParameter<DoubleValue>("StartTemperature", "The initial temperature.", new DoubleValue(100)));
      Parameters.Add(new ValueParameter<DoubleValue>("EndTemperature", "The final temperature which should be reached when iterations reaches maximum iterations.", new DoubleValue(1e-6)));
      Parameters.Add(new ConstrainedValueParameter<IDiscreteDoubleValueModifier>("AnnealingOperator", "The operator used to modify the temperature."));
      Parameters.Add(new LookupParameter<ResultCollection>("Results", "The variable where the results are stored."));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>("Quality", "The quality/fitness value of a solution."));

      foreach (IDiscreteDoubleValueModifier op in ApplicationManager.Manager.GetInstances<IDiscreteDoubleValueModifier>().OrderBy(x => x.Name))
        AnnealingOperatorParameter.ValidValues.Add(op);

      ParameterizeAnnealingOperators();
      ParameterizeSAMainLoop();

      RegisterEventHandlers();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SimulatedAnnealingImprovementOperator(this, cloner);
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      RegisterEventHandlers();
    }

    #region Event Handler Registration
    private void RegisterEventHandlers() {
      MoveGeneratorParameter.ValueChanged += new EventHandler(MoveGeneratorParameter_ValueChanged);
      if (problem != null)
        RegisterProblemEventHandlers();
    }

    private void RegisterProblemEventHandlers() {
      problem.Reset += new EventHandler(problem_Reset);
      problem.OperatorsChanged += new EventHandler(problem_OperatorsChanged);
    }

    private void DeregisterProblemEventHandlers() {
      problem.Reset -= new EventHandler(problem_Reset);
      problem.OperatorsChanged -= new EventHandler(problem_OperatorsChanged);
    }
    #endregion

    #region Event Handlers
    private void MoveGeneratorParameter_ValueChanged(object sender, EventArgs e) {
      ChooseMoveOperators();
      ParameterizeSAMainLoop();
    }

    private void problem_Reset(object sender, EventArgs e) {
      UpdateProblem();
    }

    private void problem_OperatorsChanged(object sender, EventArgs e) {
      UpdateProblem();
    }
    #endregion

    private void ParameterizeAnnealingOperators() {
      foreach (IDiscreteDoubleValueModifier op in AnnealingOperatorParameter.ValidValues) {
        op.IndexParameter.ActualName = "LocalIterations";
        op.StartIndexParameter.Value = new IntValue(0);
        op.EndIndexParameter.ActualName = MaximumIterationsParameter.Name;
        op.ValueParameter.ActualName = "Temperature";
        op.StartValueParameter.ActualName = StartTemperatureParameter.Name;
        op.EndValueParameter.ActualName = EndTemperatureParameter.Name;
      }
    }

    public void UpdateProblem() {
      UpdateMoveOperators();
      ChooseMoveOperators();

      ParameterizeMoveGenerators();

      ParameterizeSAMainLoop();
      ParameterizeAnalyzers();
      UpdateAnalyzers();
    }

    private void ParameterizeAnalyzers() {
      qualityAnalyzer.ResultsParameter.ActualName = "Results";
      if (problem != null) {
        qualityAnalyzer.MaximizationParameter.ActualName = problem.MaximizationParameter.Name;
        qualityAnalyzer.QualityParameter.ActualName = problem.Evaluator.QualityParameter.Name;
        qualityAnalyzer.QualityParameter.Depth = 0;
        qualityAnalyzer.BestKnownQualityParameter.ActualName = problem.BestKnownQualityParameter.Name;
      }
    }

    private void ParameterizeSAMainLoop() {
      loop.AnalyzerParameter.ActualName = AnalyzerParameter.Name;
      loop.EvaluatedMovesParameter.ActualName = EvaluatedSolutionsParameter.Name;
      loop.IterationsParameter.ActualName = "LocalIterations";
      loop.MaximumIterationsParameter.ActualName = MaximumIterationsParameter.Name;
      loop.MoveEvaluatorParameter.ActualName = MoveEvaluatorParameter.Name;
      loop.MoveGeneratorParameter.ActualName = MoveGeneratorParameter.Name;
      loop.MoveMakerParameter.ActualName = MoveMakerParameter.Name;
      loop.QualityParameter.ActualName = QualityParameter.Name;
      loop.RandomParameter.ActualName = RandomParameter.Name;
      loop.ResultsParameter.ActualName = ResultsParameter.Name;

      if (problem != null) {
        loop.BestKnownQualityParameter.ActualName = problem.BestKnownQualityParameter.Name;
        loop.MaximizationParameter.ActualName = problem.MaximizationParameter.Name;
      }
      if (MoveEvaluator != null) {
        loop.MoveQualityParameter.ActualName = MoveEvaluator.MoveQualityParameter.ActualName;
      }
    }

    private bool IsSubclassOfGeneric(Type generic, Type toCheck) {
      while (toCheck != typeof(object)) {
        var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
        if (generic == cur) {
          return true;
        }
        toCheck = toCheck.BaseType;
      }
      return false;
    }

    private void UpdateAnalyzers() {
      Analyzer.Operators.Clear();
      if (problem != null) {
        foreach (IAnalyzer analyzer in problem.Operators.OfType<IAnalyzer>()) {
          if (!IsSubclassOfGeneric(typeof(AlleleFrequencyAnalyzer<>), analyzer.GetType()) &&
              !(analyzer is SingleObjectivePopulationDiversityAnalyzer)) {
            IAnalyzer clone = analyzer.Clone() as IAnalyzer;
            foreach (IScopeTreeLookupParameter param in clone.Parameters.OfType<IScopeTreeLookupParameter>())
              param.Depth = 0;
            Analyzer.Operators.Add(clone, false);
          }
        }
      }
      Analyzer.Operators.Add(qualityAnalyzer, false);
    }

    private void UpdateMoveOperators() {
      IMoveGenerator oldMoveGenerator = MoveGenerator;
      IMoveMaker oldMoveMaker = MoveMaker;
      ISingleObjectiveMoveEvaluator oldMoveEvaluator = MoveEvaluator;

      ClearMoveParameters();

      if (problem != null) {
        foreach (IMultiMoveGenerator generator in problem.Operators.OfType<IMultiMoveGenerator>().OrderBy(x => x.Name))
          MoveGeneratorParameter.ValidValues.Add(generator);
        foreach (IExhaustiveMoveGenerator generator in problem.Operators.OfType<IExhaustiveMoveGenerator>().OrderBy(x => x.Name))
          MoveGeneratorParameter.ValidValues.Add(generator);

        if (oldMoveGenerator != null) {
          IMoveGenerator newMoveGenerator = MoveGeneratorParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldMoveGenerator.GetType());
          if (newMoveGenerator != null) MoveGenerator = newMoveGenerator;
        }

        ChooseMoveOperators(oldMoveMaker, oldMoveEvaluator);
      }
    }

    private void ChooseMoveOperators(IMoveMaker oldMoveMaker = null, ISingleObjectiveMoveEvaluator oldMoveEvaluator = null) {
      if (oldMoveMaker == null) oldMoveMaker = MoveMaker;
      if (oldMoveEvaluator == null) oldMoveEvaluator = MoveEvaluator;
      MoveMakerParameter.ValidValues.Clear();
      MoveEvaluatorParameter.ValidValues.Clear();

      if (MoveGenerator != null) {
        IMoveGenerator generator = MoveGeneratorParameter.Value;
        foreach (IMoveMaker moveMaker in MoveHelper.GetCompatibleMoveMakers(generator, Problem.Operators.OfType<IOperator>()).OrderBy(x => x.Name))
          MoveMakerParameter.ValidValues.Add(moveMaker);
        foreach (ISingleObjectiveMoveEvaluator moveEvaluator in MoveHelper.GetCompatibleSingleObjectiveMoveEvaluators(generator, Problem.Operators.OfType<IOperator>()).OrderBy(x => x.Name))
          MoveEvaluatorParameter.ValidValues.Add(moveEvaluator);

        if (oldMoveMaker != null) {
          IMoveMaker mm = MoveMakerParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldMoveMaker.GetType());
          if (mm != null) MoveMaker = mm;
        }
        if (oldMoveEvaluator != null) {
          ISingleObjectiveMoveEvaluator me = MoveEvaluatorParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldMoveEvaluator.GetType());
          if (me != null) MoveEvaluator = me;
        }
      }
    }

    private void ClearMoveParameters() {
      MoveGeneratorParameter.ValidValues.Clear();
      MoveMakerParameter.ValidValues.Clear();
      MoveEvaluatorParameter.ValidValues.Clear();
    }

    private void ParameterizeMoveGenerators() {
      if (problem != null) {
        foreach (IMultiMoveGenerator generator in problem.Operators.OfType<IMultiMoveGenerator>())
          generator.SampleSizeParameter.ActualName = InnerIterationsParameter.Name;
      }
    }

    public override IOperation Apply() {
      IScope currentScope = ExecutionContext.Scope;

      Scope localScope = new Scope();
      Scope individual = new Scope();

      foreach (IVariable var in currentScope.Variables)
        individual.Variables.Add(var); // add reference to variable otherwise the analyzer fails (it's looking down the tree)

      localScope.SubScopes.Add(individual);
      currentScope.SubScopes.Add(localScope);
      int index = currentScope.SubScopes.Count - 1;

      SubScopesProcessor processor = new SubScopesProcessor();
      SubScopesRemover remover = new SubScopesRemover();

      remover.RemoveAllSubScopes = false;
      remover.SubScopeIndexParameter.Value = new IntValue(index);

      if (index > 0) {
        EmptyOperator eo = new EmptyOperator();
        for (int i = 0; i < index - 1; i++) {
          processor.Operators.Add(eo);
        }
      }

      VariableCreator variableCreator = new VariableCreator();
      variableCreator.CollectedValues.Add(new ValueParameter<IntValue>(loop.IterationsParameter.ActualName, new IntValue(0)));

      variableCreator.Successor = loop;

      processor.Operators.Add(variableCreator);
      processor.Successor = remover;

      OperationCollection next = new OperationCollection(base.Apply());
      next.Insert(0, ExecutionContext.CreateChildOperation(processor));

      return next;
    }
  }
}