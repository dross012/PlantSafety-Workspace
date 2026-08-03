// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Genetic-algorithm predictive scheduling optimization strategy. Evolves a
// population of task-ordering permutations, decoding each chromosome via greedy placement and
// scoring it by coverage/violation/makespan fitness, using tournament selection, order
// crossover, swap mutation, and elitism. Fully deterministic given a seed, so the strategy
// remains unit-testable. SchemaVersion: 1

using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Services;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Optimization;

/// <summary>
/// Genetic-algorithm strategy: evolves a population of task-ordering permutations (chromosomes),
/// decoding each via greedy placement and selecting/breeding by a coverage/violation/makespan
/// fitness function. Deterministic for a fixed seed.
/// </summary>
public sealed class GeneticAlgorithmOptimizationStrategy : IOptimizationStrategy
{
    private const int PopulationSize = 24;
    private const int Generations = 30;
    private const int TournamentSize = 3;
    private const double MutationRate = 0.12;
    private const int ElitismCount = 2;

    private readonly ResourceAvailabilityModel _availabilityModel;
    private readonly ConstraintEvaluator _constraintEvaluator;
    private readonly int _seed;

    /// <summary>Creates the strategy with the platform-default rule engine components and a fixed deterministic seed.</summary>
    public GeneticAlgorithmOptimizationStrategy() : this(new ResourceAvailabilityModel(), new ConstraintEvaluator(), seed: 20260802) { }

    /// <summary>Creates the strategy with explicit rule engine collaborators and a deterministic seed (for testing/composition).</summary>
    public GeneticAlgorithmOptimizationStrategy(ResourceAvailabilityModel availabilityModel, ConstraintEvaluator constraintEvaluator, int seed)
    {
        _availabilityModel = availabilityModel;
        _constraintEvaluator = constraintEvaluator;
        _seed = seed;
    }

    /// <inheritdoc/>
    public string StrategyName => "Genetic";

    /// <inheritdoc/>
    public OptimizationResult Optimize(OptimizationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var tasks = context.Tasks;
        if (tasks.Count == 0)
        {
            return new OptimizationResult
            {
                StrategyName = StrategyName,
                Assignments = [],
                UnscheduledTaskIds = [],
                Score = 1.0,
            };
        }

        var random = new Random(_seed);
        var population = InitializePopulation(tasks.Count, random);

        (int[] Chromosome, DecodedSchedule Decoded, double Fitness) best = default;

        for (var generation = 0; generation < Generations; generation++)
        {
            var evaluated = population
                .Select(chromosome =>
                {
                    var decoded = Decode(chromosome, tasks, context);
                    var fitness = ComputeFitness(decoded, tasks.Count);
                    return (Chromosome: chromosome, Decoded: decoded, Fitness: fitness);
                })
                .OrderByDescending(x => x.Fitness)
                .ToList();

            if (best.Chromosome is null || evaluated[0].Fitness > best.Fitness)
                best = evaluated[0];

            var nextGeneration = evaluated.Take(ElitismCount).Select(x => x.Chromosome).ToList();

            while (nextGeneration.Count < PopulationSize)
            {
                var parentA = TournamentSelect(evaluated, random);
                var parentB = TournamentSelect(evaluated, random);
                var child = OrderCrossover(parentA, parentB, random);
                Mutate(child, random);
                nextGeneration.Add(child);
            }

            population = nextGeneration;
        }

        return new OptimizationResult
        {
            StrategyName = StrategyName,
            Assignments = best.Decoded.NewAssignments,
            UnscheduledTaskIds = best.Decoded.Unscheduled,
            ToleratedViolations = best.Decoded.Tolerated,
            Score = Math.Round(best.Fitness, 4),
        };
    }

    private static List<int[]> InitializePopulation(int taskCount, Random random)
    {
        var population = new List<int[]>(PopulationSize);
        var basePermutation = Enumerable.Range(0, taskCount).ToArray();

        for (var i = 0; i < PopulationSize; i++)
        {
            var permutation = (int[])basePermutation.Clone();
            Shuffle(permutation, random);
            population.Add(permutation);
        }

        return population;
    }

    private static void Shuffle(int[] array, Random random)
    {
        for (var i = array.Length - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    private sealed record DecodedSchedule(
        IReadOnlyList<ScheduleAssignment> NewAssignments,
        IReadOnlyList<SchedulableTaskId> Unscheduled,
        IReadOnlyList<ConstraintViolation> Tolerated);

    private DecodedSchedule Decode(int[] chromosome, IReadOnlyList<SchedulableTask> tasks, OptimizationContext context)
    {
        var committed = new List<ScheduleAssignment>(context.ExistingAssignments);
        var unscheduled = new List<SchedulableTaskId>();
        var tolerated = new List<ConstraintViolation>();

        foreach (var geneIndex in chromosome)
        {
            var task = tasks[geneIndex];
            var resourceId = task.RequiredMachineId ?? task.RequiredCrewId;

            if (resourceId is null)
            {
                unscheduled.Add(task.TaskId);
                continue;
            }

            var placed = false;
            foreach (var candidateStart in _availabilityModel.EnumerateCandidateStartTimes(
                         resourceId.Value, task.EstimatedDuration, task.EarliestStartUtc, task.LatestFinishUtc,
                         context.Availability, committed))
            {
                var candidateEnd = UtcTimestamp.From(candidateStart.Value + task.EstimatedDuration);
                var candidate = new ScheduleAssignment(task.TaskId, resourceId.Value, candidateStart, candidateEnd)
                {
                    EffectivePriorityWeight = task.BasePriorityWeight,
                };

                var violations = _constraintEvaluator.Evaluate(task, candidate, context.Availability, committed);
                if (violations.Any(v => v.Severity == ConstraintSeverity.Blocking))
                    continue;

                committed.Add(candidate);
                tolerated.AddRange(violations);
                placed = true;
                break;
            }

            if (!placed)
                unscheduled.Add(task.TaskId);
        }

        var newAssignments = committed.Except(context.ExistingAssignments).ToList();
        return new DecodedSchedule(newAssignments, unscheduled, tolerated);
    }

    private static double ComputeFitness(DecodedSchedule decoded, int totalTasks)
    {
        if (totalTasks == 0)
            return 1.0;

        var coverage = decoded.NewAssignments.Count / (double)totalTasks;
        var violationPenalty = decoded.Tolerated.Count * 0.02;

        var makespan = decoded.NewAssignments.Count == 0
            ? TimeSpan.Zero
            : decoded.NewAssignments.Max(a => a.EndUtc.Value) - decoded.NewAssignments.Min(a => a.StartUtc.Value);

        var makespanPenalty = Math.Min(0.2, makespan.TotalDays * 0.01);

        return Math.Max(0.0, coverage - violationPenalty - makespanPenalty);
    }

    private static int[] TournamentSelect(List<(int[] Chromosome, DecodedSchedule Decoded, double Fitness)> evaluated, Random random)
    {
        var bestIndex = random.Next(evaluated.Count);
        for (var i = 1; i < TournamentSize; i++)
        {
            var candidateIndex = random.Next(evaluated.Count);
            if (evaluated[candidateIndex].Fitness > evaluated[bestIndex].Fitness)
                bestIndex = candidateIndex;
        }

        return evaluated[bestIndex].Chromosome;
    }

    /// <summary>Order crossover (OX): preserves a contiguous slice from parent A, fills the remainder in parent B's relative order.</summary>
    private static int[] OrderCrossover(int[] parentA, int[] parentB, Random random)
    {
        var length = parentA.Length;
        var child = new int[length];
        Array.Fill(child, -1);

        var start = random.Next(length);
        var end = random.Next(length);
        if (start > end)
            (start, end) = (end, start);

        for (var i = start; i <= end; i++)
            child[i] = parentA[i];

        var usedValues = new HashSet<int>(child.Where(v => v >= 0));
        var insertPosition = (end + 1) % length;

        foreach (var gene in parentB)
        {
            if (usedValues.Contains(gene))
                continue;

            child[insertPosition] = gene;
            usedValues.Add(gene);
            insertPosition = (insertPosition + 1) % length;
        }

        return child;
    }

    private static void Mutate(int[] chromosome, Random random)
    {
        if (random.NextDouble() > MutationRate || chromosome.Length < 2)
            return;

        var i = random.Next(chromosome.Length);
        var j = random.Next(chromosome.Length);
        (chromosome[i], chromosome[j]) = (chromosome[j], chromosome[i]);
    }
}
