/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               PendingTrainingJobSpecification.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Interfaces;
using Intelligence.Domain.AITraining.Aggregates;
using Intelligence.Domain.AITraining.ValueObjects;

namespace Intelligence.Domain.AITraining.Specifications;

/// <summary>Satisfied when a <see cref="TrainingJob"/> is pending dispatch and has a registered dataset.</summary>
public sealed class PendingTrainingJobSpecification : ISpecification<TrainingJob>
{
    /// <inheritdoc />
    public string Description =>
        "TrainingJob must be in Pending status with a registered dataset to be eligible for dispatch.";

    /// <inheritdoc />
    public bool IsSatisfiedBy(TrainingJob entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return entity.Status == TrainingJobStatus.Pending && entity.Dataset is not null;
    }
}
