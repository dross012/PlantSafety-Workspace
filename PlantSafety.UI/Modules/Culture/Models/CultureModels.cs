// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Culture dashboard module. Backed
//   by 4 PlantSafety.Api Analytics Culture read-models (CultureIndex,
//   Behavior, EngagementScoring, TrendModeling) — each wraps its payload in
//   the platform-wide ApiResponse<T> shape (Success/Data/Errors/Metadata).
//   ReadModelEnvelopeDto/ReadModelPayloadDto mirror that wire shape exactly
//   so SafetyApiClientBase.GetAsync<T> can deserialize it directly. No
//   domain logic present — display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Culture.Models;

/// <summary>A single composite safety culture index snapshot for a given culture dimension.</summary>
public sealed record CultureIndexItemDto
{
    public Guid IndexSnapshotId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    /// <summary>Culture dimension measured (e.g., Trust, Communication, Accountability).</summary>
    public string CultureDimension { get; init; } = string.Empty;
    /// <summary>Composite index score for this dimension.</summary>
    public decimal IndexScore { get; init; }
    /// <summary>Trend direction versus the prior period (e.g., Improving, Stable, Declining).</summary>
    public string TrendDirection { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CalculatedAtUtc { get; init; }
}

/// <summary>A single behavior observation recorded as part of the safety culture program.</summary>
public sealed record BehaviorItemDto
{
    public Guid ObservationId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public Guid EmployeeId { get; init; }
    /// <summary>Behavior category (e.g., PPE Compliance, Housekeeping, Ergonomics).</summary>
    public string BehaviorCategory { get; init; } = string.Empty;
    /// <summary>Observation classification (e.g., Safe, AtRisk).</summary>
    public string ObservationType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime ObservedAtUtc { get; init; }
    public Guid ObservedBy { get; init; }
}

/// <summary>A single engagement score recorded for an employee.</summary>
public sealed record EngagementScoringItemDto
{
    public Guid ScoreId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public Guid EmployeeId { get; init; }
    public decimal EngagementScore { get; init; }
    /// <summary>Category of the scoring source (e.g., Survey, Participation, Recognition).</summary>
    public string ScoreCategory { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime ScoredAtUtc { get; init; }
}

/// <summary>A single predicted culture trend data point produced by the trend model.</summary>
public sealed record TrendModelingItemDto
{
    public Guid TrendModelId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    /// <summary>Name of the culture metric being modeled.</summary>
    public string MetricName { get; init; } = string.Empty;
    public decimal PredictedValue { get; init; }
    /// <summary>Model confidence for the prediction, expressed as a 0-1 value.</summary>
    public decimal ModelConfidence { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime GeneratedAtUtc { get; init; }
}

/// <summary>Read-model payload envelope, matching PlantSafety.Api's per-endpoint Response records.</summary>
/// <typeparam name="TItem">The item type contained in <see cref="Items"/>.</typeparam>
public sealed record ReadModelPayloadDto<TItem>
{
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public DateTime GeneratedAt { get; init; }
    public IReadOnlyList<TItem> Items { get; init; } = [];
}

/// <summary>
/// Client-side mirror of PlantSafety.Api.Models.ApiResponse&lt;T&gt; — the platform-wide
/// controller envelope (Success/Data/Errors/Metadata) used by all Compliance/Operational/
/// CrossDomain/Analytics read-model endpoints.
/// </summary>
/// <typeparam name="TItem">The item type contained in the payload's <c>Items</c> list.</typeparam>
public sealed record ReadModelEnvelopeDto<TItem>
{
    public bool Success { get; init; }
    public ReadModelPayloadDto<TItem>? Data { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
}
