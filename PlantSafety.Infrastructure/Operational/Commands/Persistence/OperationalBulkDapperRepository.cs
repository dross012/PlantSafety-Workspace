// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Commands
//  Layer:          Infrastructure
//  File:           OperationalBulkDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository backing IOperationalBulkRepository.BulkUpdateAsync. Applies the
//                  requested operation to each resource id individually within its own transaction
//                  so that a single item failure does not abort the remainder of the batch, matching
//                  the partial-success contract documented on IOperationalBulkRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.Commands;

namespace PlantSafety.Infrastructure.Operational.Commands.Persistence;

/// <summary>
/// Dapper-backed repository applying bulk operations across Operational resources.
/// Executes a write stored procedure once per resource id, within its own transaction.
/// </summary>
public sealed class OperationalBulkDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    ILogger<OperationalBulkDapperRepository> logger)
    : IOperationalBulkRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<BulkOperationalItemResult>> BulkUpdateAsync(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<Guid> resourceIds,
        string operation,
        Dictionary<string, string>? parameters,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "BulkUpdate {Operation} for {Count} Operational resources, tenant {TenantId}",
            operation, resourceIds.Count, tenantId);

        Guid? assignedToUserId = null;
        if (parameters is not null
            && parameters.TryGetValue("AssignedToUserId", out var assignedToUserIdRaw)
            && Guid.TryParse(assignedToUserIdRaw, out var parsedAssignedToUserId))
        {
            assignedToUserId = parsedAssignedToUserId;
        }

        var notes = parameters is not null && parameters.TryGetValue("Notes", out var notesRaw)
            ? notesRaw
            : null;

        var results = new List<BulkOperationalItemResult>(resourceIds.Count);
        var nowUtc = DateTime.UtcNow;

        using var conn = connectionFactory.CreateConnection();

        foreach (var resourceId in resourceIds)
        {
            using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var sqlParameters = new StoredProcedureBuilder()
                    .AddGuid("TenantId", tenantId)
                    .AddGuid("SiteId", siteId)
                    .AddGuid("ResourceId", resourceId)
                    .AddString("Operation", operation, 50)
                    .AddGuid("AssignedToUserId", assignedToUserId)
                    .AddString("Notes", notes, 4000)
                    .AddDateTime("NowUtc", nowUtc)
                    .Build();

                var rowsAffected = await commandExecutor.ExecuteAsync(
                    conn,
                    "[Operational].[usp_OperationalResources_BulkUpdate]",
                    sqlParameters,
                    tx,
                    cancellationToken: cancellationToken);

                tx.Commit();

                results.Add(rowsAffected > 0
                    ? new BulkOperationalItemResult(resourceId, true)
                    : new BulkOperationalItemResult(resourceId, false, "Resource not found for tenant/site.", "NOT_FOUND"));
            }
            catch (Exception ex)
            {
                tx.Rollback();
                logger.LogError(ex, "BulkUpdate {Operation} failed for resource {ResourceId}", operation, resourceId);
                results.Add(new BulkOperationalItemResult(resourceId, false, "Unable to update resource.", "UPDATE_FAILED"));
            }
        }

        return results.AsReadOnly();
    }
}
