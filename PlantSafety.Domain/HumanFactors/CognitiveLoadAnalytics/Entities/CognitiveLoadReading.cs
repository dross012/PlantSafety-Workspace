// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveLoadReading.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable child entity of CognitiveLoadLedger recording a single worker
//                  cognitive-workload measurement. Data governance: this record carries individually
//                  identifiable worker data (WorkerUserId, CognitiveLoadScore) and is treated as
//                  sensitive personal data -- access is restricted at the query/authorization layer
//                  and any cross-worker dashboard aggregates rather than surfacing per-worker rows.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Entities;

/// <summary>A single worker cognitive-workload measurement recorded on a <see cref="Aggregates.CognitiveLoadLedger"/>.</summary>
public sealed class CognitiveLoadReading
{
    /// <summary>Unique identity of this reading.</summary>
    public CognitiveLoadReadingId CognitiveLoadReadingId { get; private set; }

    /// <summary>Identity of the worker this reading was measured for.</summary>
    public UserId WorkerUserId { get; private set; }

    /// <summary>Optional reference to the task complexity profile in force when this reading was taken.</summary>
    public TaskComplexityProfileId? TaskComplexityProfileId { get; private set; }

    /// <summary>Measured cognitive workload score, 0-100 (e.g. NASA-TLX composite score).</summary>
    public decimal CognitiveLoadScore { get; private set; }

    /// <summary>Classification of <see cref="CognitiveLoadScore"/> per <see cref="Policies.CognitiveOverloadPolicy"/>.</summary>
    public CognitiveWorkloadLevel WorkloadLevel { get; private set; }

    /// <summary>Measurement instrument/method (e.g. "NASA-TLX", "Physiological", "SelfReport").</summary>
    public string MeasurementMethod { get; private set; } = string.Empty;

    /// <summary>UTC instant the underlying measurement was taken.</summary>
    public UtcTimestamp MeasuredAtUtc { get; private set; }

    /// <summary>UTC instant this reading was recorded in the ledger.</summary>
    public UtcTimestamp RecordedAtUtc { get; private set; }

    /// <summary>Identity of the user who recorded this entry.</summary>
    public UserId RecordedBy { get; private set; }

    /// <summary>Gets a value indicating whether this reading is classified as an overload event.</summary>
    public bool IsOverload => WorkloadLevel == CognitiveWorkloadLevel.Overload;

    /// <summary>For ORM rehydration only.</summary>
    private CognitiveLoadReading() { }

    /// <summary>Creates a new cognitive-load reading.</summary>
    public CognitiveLoadReading(
        CognitiveLoadReadingId cognitiveLoadReadingId,
        UserId workerUserId,
        TaskComplexityProfileId? taskComplexityProfileId,
        decimal cognitiveLoadScore,
        CognitiveWorkloadLevel workloadLevel,
        string measurementMethod,
        UtcTimestamp measuredAtUtc,
        UtcTimestamp recordedAtUtc,
        UserId recordedBy)
    {
        if (cognitiveLoadScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(cognitiveLoadScore), "CognitiveLoadScore must be between 0 and 100.");
        if (string.IsNullOrWhiteSpace(measurementMethod))
            throw new ArgumentException("MeasurementMethod must not be null or whitespace.", nameof(measurementMethod));

        CognitiveLoadReadingId  = cognitiveLoadReadingId;
        WorkerUserId             = workerUserId;
        TaskComplexityProfileId  = taskComplexityProfileId;
        CognitiveLoadScore       = cognitiveLoadScore;
        WorkloadLevel            = workloadLevel;
        MeasurementMethod        = measurementMethod.Trim();
        MeasuredAtUtc             = measuredAtUtc;
        RecordedAtUtc             = recordedAtUtc;
        RecordedBy                = recordedBy;
    }
}
