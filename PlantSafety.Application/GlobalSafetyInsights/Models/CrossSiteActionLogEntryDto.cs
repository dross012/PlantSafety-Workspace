// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           CrossSiteActionLogEntryDto.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §5 Global Safety Dashboard)
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO projecting a single [GlobalSafetyInsights].[CrossSiteActionLog]
//                  entry -- backs the Global Safety Dashboard's corrective-action tracker and
//                  incident-timeline widgets. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.GlobalSafetyInsights.Models;

/// <summary>A single cross-site coordinated-action audit ledger entry.</summary>
public sealed record CrossSiteActionLogEntryDto
{
    /// <summary>Audit ledger entry identifier.</summary>
    public required Guid CrossSiteActionLogId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Every site affected by this coordinated action.</summary>
    public required IReadOnlyList<Guid> AffectedSiteIds { get; init; }

    /// <summary>Discriminator for the kind of coordinated action (e.g. "CorrectiveActionRollout", "IncidentEscalation").</summary>
    public required string ActionType { get; init; }

    /// <summary>Human-readable description of the action.</summary>
    public required string Description { get; init; }

    /// <summary>Identity of the user who initiated the action, if any.</summary>
    public Guid? InitiatedByUserId { get; init; }

    /// <summary>UTC instant the action was recorded.</summary>
    public required DateTime RecordedAtUtc { get; init; }

    /// <summary>The region the action was resolved/routed in, if known.</summary>
    public string? RegionCode { get; init; }
}
