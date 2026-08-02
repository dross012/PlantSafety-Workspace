// ============================================================================
// File: DataLakeActivityCommandRequests.cs
// Project: PlantSafety.Api
// Module: Intelligence.DataLake
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the Intelligence.DataLake V1
//              controller's Phase 4 full build-out lifecycle actions
//              (Create -> StartProcessing -> Curate/Fail -> Archive). Mirrors
//              PlantSafety.Api.ContinuousImprovement.RCA.Contracts.Requests.RCACommandRequests.cs
//              -- every mutating lifecycle request carries an optional ETag field so clients can
//              supply the optimistic-concurrency token captured from a prior read (e.g.
//              DataLakeActivityDetailDto.ETag); a stale value results in a concurrency-conflict
//              error.
// ============================================================================

namespace PlantSafety.Api.Intelligence.DataLake.Contracts.Requests;

/// <summary>Request body for creating a new Intelligence.DataLake activity record.</summary>
public sealed record CreateDataLakeActivityRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the dataset associated with this record.</summary>
    public required string DatasetName { get; init; }

    /// <summary>Data lake zone the record resides in (e.g., Raw/Cleansed/Curated/ML/Sandbox).</summary>
    public required string Zone { get; init; }

    /// <summary>Number of records processed in this activity.</summary>
    public long? RecordCount { get; init; }
}

/// <summary>Request body for beginning processing of an ingested data lake activity (Ingested -&gt; Processing).</summary>
public sealed record StartProcessingDataLakeActivityRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this activity record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for marking a data lake activity's data as curated (Processing -&gt; Curated).</summary>
public sealed record CurateDataLakeActivityRequest
{
    /// <summary>Final record count, if it differs from the count recorded at ingestion.</summary>
    public long? FinalRecordCount { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this activity record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for marking a data lake activity as failed (Processing -&gt; Failed).</summary>
public sealed record FailDataLakeActivityRequest
{
    /// <summary>Reason the activity failed.</summary>
    public required string FailureReason { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this activity record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a curated or failed data lake activity for compliance retention (Curated/Failed -&gt; Archived).</summary>
public sealed record ArchiveDataLakeActivityRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this activity record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
