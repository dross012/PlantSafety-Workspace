// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Application
//  File:           AiSafetyIntelligenceItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single AiSafetyIntelligence record. Backs
//                  GetAiSafetyIntelligenceByIdQuery. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.AiSafetyIntelligence.Models;

/// <summary>A single AiSafetyIntelligence read-model record.</summary>
public sealed record AiSafetyIntelligenceItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid AiSafetyIntelligenceId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Insight classification: AnomalyDetection, PredictiveRisk, PatternRecognition.</summary>
    public required string InsightType { get; init; }

    /// <summary>Narrative description of the AI-generated insight.</summary>
    public required string Description { get; init; }

    /// <summary>Confidence score for the insight, in the range 0-1.</summary>
    public decimal ConfidenceScore { get; init; }

    /// <summary>Name of the source AI/ML model that produced this insight.</summary>
    public required string SourceModelName { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
