/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               TrainingJobDomainService.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.AITraining.Aggregates;
using Intelligence.Domain.AITraining.ValueObjects;

namespace Intelligence.Domain.AITraining.Services;

/// <summary>Stateless domain service for ML training job business rules. No I/O permitted.</summary>
public sealed class TrainingJobDomainService
{
    /// <summary>Returns true when a completed job's model meets the minimum deployment threshold.</summary>
    public bool IsEligibleForDeployment(TrainingJob job)
    {
        ArgumentNullException.ThrowIfNull(job);
        return job.Status == TrainingJobStatus.Completed
               && job.AccuracyScore is not null
               && job.AccuracyScore.MeetsDeploymentThreshold;
    }

    /// <summary>Returns true when the proposed dataset hash differs from the one already registered, indicating re-training is needed.</summary>
    public bool RequiresRetraining(TrainingJob job, DatasetHash newHash)
    {
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(newHash);
        if (job.Dataset is null) return true;
        return !job.Dataset.Hash.Equals(newHash);
    }
}
