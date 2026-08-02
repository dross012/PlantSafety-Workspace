// ============================================================================
// File: EnvironmentalHealthAggregateRequests.cs
// Project: PlantSafety.Api
// Module: EnvironmentalHealth
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the EnvironmentalHealth
//              aggregate-root V1 controller. Covers the full lifecycle:
//              Create -> Activate -> Complete -> Close -> Archive. Named distinctly
//              ("EnvironmentalHealthAggregateRequests", not "EnvironmentalHealthRequests") to
//              avoid any future collision with the pre-existing, unrelated, LIVE
//              Features.EnvironmentalHealth "reading" vertical's own request contracts. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Contracts.Requests.AuditingRequests.cs --
//              every mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.EnvironmentalHealth.Contracts.Requests;

/// <summary>Request body for submitting a new EnvironmentalHealth record.</summary>
public sealed record CreateEnvironmentalHealthRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Environmental monitoring classification (e.g. AirQuality/WaterQuality/NoiseLevel/SoilContamination).</summary>
    public required string MonitoringType { get; init; }

    /// <summary>Measured value for the monitored metric, when captured.</summary>
    public decimal? MeasuredValue { get; init; }

    /// <summary>Unit of measure for the measured value.</summary>
    public required string Unit { get; init; }

    /// <summary>Regulatory/internal threshold this measurement is evaluated against.</summary>
    public decimal Threshold { get; init; }

    /// <summary>Whether the measured value exceeded the threshold.</summary>
    public bool ThresholdExceeded { get; init; }

    /// <summary>Evaluated business classification (Normal/Warning/Critical).</summary>
    public string EvaluatedStatus { get; init; } = "Normal";
}

/// <summary>Request body for activating an EnvironmentalHealth record (Created -&gt; Active).</summary>
public sealed record ActivateEnvironmentalHealthRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this EnvironmentalHealth record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing an EnvironmentalHealth record (Active -&gt; Completed).</summary>
public sealed record CompleteEnvironmentalHealthRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this EnvironmentalHealth record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing an EnvironmentalHealth record (Completed -&gt; Closed).</summary>
public sealed record CloseEnvironmentalHealthRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this EnvironmentalHealth record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed EnvironmentalHealth record (Closed -&gt; Archived).</summary>
public sealed record ArchiveEnvironmentalHealthRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this EnvironmentalHealth record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
