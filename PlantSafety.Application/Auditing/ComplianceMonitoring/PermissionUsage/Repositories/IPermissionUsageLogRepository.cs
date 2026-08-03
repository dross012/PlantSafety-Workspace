// AUTHOR: PlantSafety Platform Team (Claude Code performance-optimization-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Write-side repository for permission-usage/access-log capture (Handbook
//                    Phase 6 §5.4 Compliance Monitoring). Implemented in the Infrastructure layer
//                    against a dedicated [Auditing].[PermissionUsageLog] table -- kept separate
//                    from the cross-database, deliberately READ-ONLY [PlantSafety.AuditDB].
//                    dbo.AuditLog (see Shared.AuditLogging.Abstractions.IAuditLogRepository
//                    remarks) rather than bolting a write path onto a table that has neither the
//                    required columns (PermissionName/Decision) nor a write contract today.
// SchemaVersion: 1

using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Models;

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Repositories;

/// <summary>Append-only write repository for authorization-decision (permission usage) log entries.</summary>
public interface IPermissionUsageLogRepository
{
    /// <summary>Records a single authorization decision. Best-effort: MUST NOT fault the calling request.</summary>
    Task RecordAsync(PermissionUsageLogEntry entry, CancellationToken cancellationToken);
}
