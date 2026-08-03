// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           IGlobalSafetyActionAuditRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §4 Event Mesh Extensions)
//  SchemaVersion:  2026.1
//  Description:    Application-layer repository abstraction for the CrossSiteActionEvent audit
//                  ledger (module-level, not attached to a single snapshot aggregate instance --
//                  see RecordCrossSiteActionCommandHandler remarks for why). Implemented in
//                  Infrastructure. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.GlobalSafetyInsights.Models;

namespace PlantSafety.Application.GlobalSafetyInsights.Repositories;

/// <summary>Append-only audit ledger for cross-site coordinated actions.</summary>
public interface IGlobalSafetyActionAuditRepository
{
    /// <summary>Records a cross-site action into the audit ledger.</summary>
    Task RecordAsync(
        Guid tenantId,
        IReadOnlyList<Guid> affectedSiteIds,
        string actionType,
        string description,
        Guid? initiatedByUserId,
        DateTime recordedAtUtc,
        string? regionCode,
        Guid? workflowInstanceId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lists cross-site action audit entries for a tenant within [<paramref name="from"/>,
    /// <paramref name="to"/>], most recent first -- backs the Global Safety Dashboard's
    /// corrective-action tracker / incident-timeline widgets (Phase 8 §5).
    /// </summary>
    Task<IReadOnlyList<CrossSiteActionLogEntryDto>> ListAsync(
        Guid tenantId, DateTime from, DateTime to, CancellationToken cancellationToken);
}
