// ============================================================================
// File: AiSafetyIntelligenceRequests.cs
// Project: PlantSafety.Api
// Module: AiSafetyIntelligence
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the AiSafetyIntelligence V1
//              controller. Covers the full lifecycle: Create -> Activate -> Complete -> Close ->
//              Archive. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Contracts.Requests.AuditingRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field.
// ============================================================================

namespace PlantSafety.Api.AiSafetyIntelligence.Contracts.Requests;

/// <summary>Request body for registering a new AiSafetyIntelligence record.</summary>
public sealed record CreateAiSafetyIntelligenceRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Insight classification (AnomalyDetection/PredictiveRisk/PatternRecognition).</summary>
    public string InsightType { get; init; } = "AnomalyDetection";

    /// <summary>Narrative description of the AI-generated insight.</summary>
    public required string Description { get; init; }

    /// <summary>Confidence score for the insight, in the range 0-1.</summary>
    public required decimal ConfidenceScore { get; init; }

    /// <summary>Name of the source AI/ML model that produced this insight.</summary>
    public required string SourceModelName { get; init; }
}

/// <summary>Request body for activating an AiSafetyIntelligence record (Created -&gt; Active).</summary>
public sealed record ActivateAiSafetyIntelligenceRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for completing an AiSafetyIntelligence record (Active -&gt; Completed).</summary>
public sealed record CompleteAiSafetyIntelligenceRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing an AiSafetyIntelligence record (Completed -&gt; Closed).</summary>
public sealed record CloseAiSafetyIntelligenceRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving an AiSafetyIntelligence record (Closed -&gt; Archived).</summary>
public sealed record ArchiveAiSafetyIntelligenceRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
