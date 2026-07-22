// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.Commands
//  Layer:          Infrastructure
//  File:           SchedulingBulkDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of ISchedulingBulkRepository. Applies a
//                  Status transition to each resource id, searching across every Scheduling
//                  aggregate table server-side via usp_Scheduling_BulkUpdateResource.
//                  Partial-success semantics: per-item failures are captured and
//                  reported, never thrown. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Scheduling.Commands;

namespace PlantSafety.Infrastructure.Scheduling.Commands.Persistence;

/// <summary>Dapper-backed bulk-operation repository for the Scheduling module.</summary>
public sealed class SchedulingBulkDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    ILogger<SchedulingBulkDapperRepository> logger)
    : ISchedulingBulkRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<BulkSchedulingItemResult>> BulkUpdateAsync(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<Guid> resourceIds,
        string operation,
        Dictionary<string, string>? parameters,
        CancellationToken cancellationToken)
    {
        var results = new List<BulkSchedulingItemResult>();

        using var conn = connectionFactory.CreateConnection();

        foreach (var resourceId in resourceIds)
        {
            try
            {
                using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

                var sp = new DynamicParameters();
                sp.Add("ResourceId", resourceId, DbType.Guid);
                sp.Add("TenantId", tenantId, DbType.Guid);
                sp.Add("SiteId", siteId, DbType.Guid);
                sp.Add("Operation", operation, DbType.String, size: 50);
                sp.Add("ActorId", (Guid?)null, DbType.Guid);
                sp.Add("NowUtc", DateTime.UtcNow, DbType.DateTime2);
                sp.Add("Found", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                await commandExecutor.ExecuteAsync(conn, "[Scheduling].[usp_Scheduling_BulkUpdateResource]", sp, tx, cancellationToken: cancellationToken);
                tx.Commit();

                var found = sp.Get<bool>("Found");
                results.Add(found
                    ? new BulkSchedulingItemResult(resourceId, true)
                    : new BulkSchedulingItemResult(resourceId, false, "Resource not found.", "NOT_FOUND"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Bulk Scheduling operation {Operation} failed for resource {ResourceId}", operation, resourceId);
                results.Add(new BulkSchedulingItemResult(resourceId, false, "Unable to apply the operation to this resource.", "OPERATION_FAILED"));
            }
        }

        return results;
    }
}
