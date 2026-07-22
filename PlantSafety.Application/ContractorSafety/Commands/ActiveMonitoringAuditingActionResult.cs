// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for ActiveMonitoringAuditing lifecycle commands ("ContractorSafety.ActiveMonitoringAuditing").
// SchemaVersion: 1

namespace PlantSafety.Application.ContractorSafety.Commands;

/// <summary>Result of a successful ActiveMonitoringAuditing lifecycle command execution.</summary>
/// <param name="ActiveMonitoringAuditingId">Identity of the affected ActiveMonitoringAuditing aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record ActiveMonitoringAuditingActionResult(
    Guid ActiveMonitoringAuditingId,
    string Status);