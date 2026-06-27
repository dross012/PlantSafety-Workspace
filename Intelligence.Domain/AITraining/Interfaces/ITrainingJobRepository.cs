/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               ITrainingJobRepository.cs
    DESCRIPTION:        Repository contract for the TrainingJob aggregate root.
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Interfaces;
using Intelligence.Domain.AITraining.Aggregates;
using Intelligence.Domain.AITraining.ValueObjects;

namespace Intelligence.Domain.AITraining.Interfaces;

/// <summary>Persistence contract for <see cref="TrainingJob"/> aggregates.</summary>
public interface ITrainingJobRepository : IRepository<TrainingJob, TrainingJobId>
{
    /// <summary>Returns all pending jobs with a registered dataset, ordered by submission date ascending.</summary>
    Task<IReadOnlyList<TrainingJob>> GetPendingJobsAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns all jobs for the given model name, ordered by submission date descending.</summary>
    Task<IReadOnlyList<TrainingJob>> GetByModelNameAsync(string modelName, CancellationToken cancellationToken = default);
}
