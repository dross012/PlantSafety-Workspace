// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           WorkloadTrendRecord.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable, append-only child entity of CognitiveLoadLedger capturing a single
//                  point-in-time workload-trend recalculation snapshot for the ledger's shift.
//                  Backs the Cognitive Load by Shift/Role dashboard and the periodic
//                  recalculate-per-shift-cycle scheduling hook. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Entities;

/// <summary>A single workload-trend recalculation snapshot on a <see cref="Aggregates.CognitiveLoadLedger"/>.</summary>
public sealed class WorkloadTrendRecord
{
    /// <summary>Unique identity of this trend record.</summary>
    public WorkloadTrendRecordId WorkloadTrendRecordId { get; private set; }

    /// <summary>Start of the window this recalculation covers.</summary>
    public UtcTimestamp PeriodStartUtc { get; private set; }

    /// <summary>End of the window this recalculation covers.</summary>
    public UtcTimestamp PeriodEndUtc { get; private set; }

    /// <summary>Mean cognitive-load score across all readings in the window.</summary>
    public decimal MeanCognitiveLoadScore { get; private set; }

    /// <summary>Count of readings classified as overload events within the window.</summary>
    public int OverloadEventCount { get; private set; }

    /// <summary>Total count of readings contributing to this recalculation.</summary>
    public int ReadingCount { get; private set; }

    /// <summary>UTC instant this recalculation was performed.</summary>
    public UtcTimestamp RecalculatedAtUtc { get; private set; }

    /// <summary>Identity of the user (or scheduled job actor) who triggered this recalculation.</summary>
    public UserId RecalculatedBy { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private WorkloadTrendRecord() { }

    /// <summary>Creates a new workload trend record.</summary>
    public WorkloadTrendRecord(
        WorkloadTrendRecordId workloadTrendRecordId,
        UtcTimestamp periodStartUtc,
        UtcTimestamp periodEndUtc,
        decimal meanCognitiveLoadScore,
        int overloadEventCount,
        int readingCount,
        UtcTimestamp recalculatedAtUtc,
        UserId recalculatedBy)
    {
        if (periodEndUtc.Value <= periodStartUtc.Value)
            throw new ArgumentException("PeriodEndUtc must be after PeriodStartUtc.", nameof(periodEndUtc));
        if (meanCognitiveLoadScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(meanCognitiveLoadScore), "MeanCognitiveLoadScore must be between 0 and 100.");
        if (overloadEventCount < 0)
            throw new ArgumentOutOfRangeException(nameof(overloadEventCount), "OverloadEventCount must not be negative.");
        if (readingCount < 0)
            throw new ArgumentOutOfRangeException(nameof(readingCount), "ReadingCount must not be negative.");

        WorkloadTrendRecordId  = workloadTrendRecordId;
        PeriodStartUtc          = periodStartUtc;
        PeriodEndUtc             = periodEndUtc;
        MeanCognitiveLoadScore  = meanCognitiveLoadScore;
        OverloadEventCount       = overloadEventCount;
        ReadingCount              = readingCount;
        RecalculatedAtUtc         = recalculatedAtUtc;
        RecalculatedBy            = recalculatedBy;
    }
}
