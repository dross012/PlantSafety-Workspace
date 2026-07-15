// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Commands
//  Layer:          Infrastructure
//  File:           ComplianceBulkDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository backing IComplianceBulkRepository.BulkUpdateAsync. Applies the
//                  requested operation to each resource id individually within its own transaction
//                  so that a single item failure does not abort the remainder of the batch, matching
//                  the partial-success contract documented on IComplianceBulkRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Commands;

namespace PlantSafety.Infrastructure.Compliance.Commands.Persistence;

/// <summary>
/// Dapper-backed repository applying bulk operations across Compliance resources.
/// Executes a write stored procedure once per resource id, within its own transaction.
/// </summary>
public sealed class ComplianceBulkDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    ILogger<ComplianceBulkDapperRepository> logger)
    : IComplianceBulkRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<BulkComplianceItemResult>> BulkUpdateAsync(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<Guid> resourceIds,
        string operation,
        Dictionary<string, string>? parameters,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "BulkUpdate {Operation} for {Count} Compliance resources, tenant {TenantId}",
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

        var results = new List<BulkComplianceItemResult>(resourceIds.Count);
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
                    "[Compliance].[usp_ComplianceResources_BulkUpdate]",
                    sqlParameters,
                    tx,
                    cancellationToken: cancellationToken);

                tx.Commit();

                results.Add(rowsAffected > 0
                    ? new BulkComplianceItemResult(resourceId, true)
                    : new BulkComplianceItemResult(resourceId, false, "Resource not found for tenant/site.", "NOT_FOUND"));
            }
            catch (Exception ex)
            {
                tx.Rollback();
                logger.LogError(ex, "BulkUpdate {Operation} failed for resource {ResourceId}", operation, resourceId);
                results.Add(new BulkComplianceItemResult(resourceId, false, "Unable to update resource.", "UPDATE_FAILED"));
            }
        }

        return results.AsReadOnly();
    }
}
