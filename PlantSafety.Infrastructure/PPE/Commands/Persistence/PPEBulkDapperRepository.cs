// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.Commands
//  Layer:          Infrastructure
//  File:           PPEBulkDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPPEBulkRepository. Applies a
//                  Status transition to each resource id, searching across every PPE
//                  aggregate table server-side via usp_PPE_BulkUpdateResource.
//                  Partial-success semantics: per-item failures are captured and
//                  reported, never thrown. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PPE.Commands;

namespace PlantSafety.Infrastructure.PPE.Commands.Persistence;

/// <summary>Dapper-backed bulk-operation repository for the PPE module.</summary>
public sealed class PPEBulkDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    ILogger<PPEBulkDapperRepository> logger)
    : IPPEBulkRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<BulkPPEItemResult>> BulkUpdateAsync(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<Guid> resourceIds,
        string operation,
        Dictionary<string, string>? parameters,
        CancellationToken cancellationToken)
    {
        var results = new List<BulkPPEItemResult>();

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

                await commandExecutor.ExecuteAsync(conn, "[PPE].[usp_PPE_BulkUpdateResource]", sp, tx, cancellationToken: cancellationToken);
                tx.Commit();

                var found = sp.Get<bool>("Found");
                results.Add(found
                    ? new BulkPPEItemResult(resourceId, true)
                    : new BulkPPEItemResult(resourceId, false, "Resource not found.", "NOT_FOUND"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Bulk PPE operation {Operation} failed for resource {ResourceId}", operation, resourceId);
                results.Add(new BulkPPEItemResult(resourceId, false, "Unable to apply the operation to this resource.", "OPERATION_FAILED"));
            }
        }

        return results;
    }
}
