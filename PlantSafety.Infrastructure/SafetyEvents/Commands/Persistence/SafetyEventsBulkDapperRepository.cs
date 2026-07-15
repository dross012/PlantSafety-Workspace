// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Commands
//  Layer:          Infrastructure
//  File:           SafetyEventsBulkDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository backing ISafetyEventsBulkRepository.BulkUpdateAsync. Applies the
//                  requested operation to each resource id individually within its own transaction
//                  so that a single item failure does not abort the remainder of the batch, matching
//                  the partial-success contract documented on ISafetyEventsBulkRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyEvents.Commands;

namespace PlantSafety.Infrastructure.SafetyEvents.Commands.Persistence;

/// <summary>
/// Dapper-backed repository applying bulk operations across SafetyEvents resources.
/// Executes a write stored procedure once per resource id, within its own transaction.
/// </summary>
public sealed class SafetyEventsBulkDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    ILogger<SafetyEventsBulkDapperRepository> logger)
    : ISafetyEventsBulkRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<BulkSafetyEventsItemResult>> BulkUpdateAsync(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<Guid> resourceIds,
        string operation,
        Dictionary<string, string>? parameters,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "BulkUpdate {Operation} for {Count} SafetyEvents resources, tenant {TenantId}",
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

        var results = new List<BulkSafetyEventsItemResult>(resourceIds.Count);
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
                    "[SafetyEvents].[usp_SafetyEventsResources_BulkUpdate]",
                    sqlParameters,
                    tx,
                    cancellationToken: cancellationToken);

                tx.Commit();

                results.Add(rowsAffected > 0
                    ? new BulkSafetyEventsItemResult(resourceId, true)
                    : new BulkSafetyEventsItemResult(resourceId, false, "Resource not found for tenant/site.", "NOT_FOUND"));
            }
            catch (Exception ex)
            {
                tx.Rollback();
                logger.LogError(ex, "BulkUpdate {Operation} failed for resource {ResourceId}", operation, resourceId);
                results.Add(new BulkSafetyEventsItemResult(resourceId, false, "Unable to update resource.", "UPDATE_FAILED"));
            }
        }

        return results.AsReadOnly();
    }
}
