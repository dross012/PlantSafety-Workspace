// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model DTOs for the SabotageIndicator subdomain.
// SchemaVersion: 1

namespace PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Models;

/// <summary>Read-model DTO for a single SabotageIndicator record. Backs the Active Sabotage Indicator Board.</summary>
public sealed record SabotageIndicatorDto(
    Guid SabotageIndicatorId,
    Guid TenantId,
    Guid SiteId,
    string Source,
    Guid? SourceReferenceId,
    string Description,
    string Severity,
    string Status,
    int CorrelatedIndicatorCount,
    DateTime RaisedAtUtc,
    string RiskManagementPriority);

/// <summary>Result of a successful RaiseSabotageIndicatorCommand.</summary>
public sealed record RaiseSabotageIndicatorResult(Guid SabotageIndicatorId, string Severity, string Status, bool AutoEscalated);
