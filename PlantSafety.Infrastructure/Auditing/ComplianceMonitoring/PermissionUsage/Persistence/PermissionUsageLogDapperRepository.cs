// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.ComplianceMonitoring.PermissionUsage
//  Layer:          Infrastructure
//  File:           PermissionUsageLogDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code performance-optimization-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository implementing IPermissionUsageLogRepository against
//                  [Auditing].[PermissionUsageLog] via usp_PermissionUsageLog_Insert. Uses its
//                  own dedicated connection (not the ambient command/query transaction) --
//                  authorization logging is a best-effort side effect that MUST NOT roll back,
//                  block on, or fault the primary command/query transaction it observes.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Models;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Repositories;

namespace PlantSafety.Infrastructure.Auditing.ComplianceMonitoring.PermissionUsage.Persistence;

/// <summary>Dapper-backed <see cref="IPermissionUsageLogRepository"/> over <c>[Auditing].[PermissionUsageLog]</c>.</summary>
public sealed class PermissionUsageLogDapperRepository(
    ISqlConnectionFactory connectionFactory,
    ILogger<PermissionUsageLogDapperRepository> logger) : IPermissionUsageLogRepository
{
    /// <inheritdoc/>
    public async Task RecordAsync(PermissionUsageLogEntry entry, CancellationToken cancellationToken)
    {
        try
        {
            using var conn = connectionFactory.CreateConnection();

            await conn.ExecuteAsync(
                "[Auditing].[usp_PermissionUsageLog_Insert]",
                new
                {
                    entry.TenantId,
                    entry.UserId,
                    entry.Permission,
                    entry.RequestType,
                    entry.Decision,
                    entry.CorrelationId,
                    entry.OccurredAtUtc
                },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            // Compliance-monitoring capture is best-effort observability; it must never fault the
            // authorization decision it is recording (Handbook §2.19-2.22: single structured error
            // envelope, no message template).
            logger.LogError(ex, "PermissionUsageLogDapperRepository failed to record decision for TenantId={TenantId} UserId={UserId}", entry.TenantId, entry.UserId);
        }
    }
}
