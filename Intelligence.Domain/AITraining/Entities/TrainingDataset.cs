/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               TrainingDataset.cs
    DESCRIPTION:        Entity: the dataset registered for a training job. Dataset hash is immutable once set.
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.AITraining.ValueObjects;

namespace Intelligence.Domain.AITraining.Entities;

/// <summary>Training dataset bound to a <see cref="Intelligence.Domain.AITraining.Aggregates.TrainingJob"/>. Immutable after registration.</summary>
public sealed class TrainingDataset : Entity<Guid>
{
    /// <summary>The training job this dataset belongs to.</summary>
    public TrainingJobId TrainingJobId { get; private init; }

    /// <summary>SHA-256 hash of dataset content. Immutable once registered.</summary>
    public DatasetHash Hash { get; private init; }

    /// <summary>Masked data source path (must not contain credentials).</summary>
    public string DataSourcePath { get; private init; }

    /// <summary>Total records in this dataset.</summary>
    public int RecordCount { get; private init; }

    /// <summary>UTC timestamp when this dataset was registered.</summary>
    public DateTime RegisteredAtUtc { get; private init; }

    private TrainingDataset(Guid id, TrainingJobId trainingJobId, DatasetHash hash, string dataSourcePath, int recordCount)
        : base(id)
    {
        TrainingJobId = trainingJobId;
        Hash = hash;
        DataSourcePath = dataSourcePath;
        RecordCount = recordCount;
        RegisteredAtUtc = DateTime.UtcNow;
    }

    /// <summary>Registers a training dataset for a training job.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when recordCount is not positive.</exception>
    public static TrainingDataset Register(
        TrainingJobId trainingJobId, DatasetHash hash, string dataSourcePath, int recordCount)
    {
        if (string.IsNullOrWhiteSpace(dataSourcePath))
            throw new ArgumentException("DataSourcePath cannot be empty.", nameof(dataSourcePath));
        if (recordCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(recordCount), "RecordCount must be positive.");

        return new TrainingDataset(Guid.NewGuid(), trainingJobId, hash, dataSourcePath.Trim(), recordCount);
    }
}
