// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.WorkerWellBeing.Records
//  Layer:          Infrastructure
//  File:           WorkerWellBeingDapperRepository.cs
//  Created:        2026-07-15
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the WorkerWellBeing read model. All DB access via
//                  stored procedures in the [WorkerWellBeing] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.WorkerWellBeing.Handlers.Records;

namespace PlantSafety.Infrastructure.Features.WorkerWellBeing.Records.Repositories;

/// <summary>
/// Dapper-backed repository for WorkerWellBeing records.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class WorkerWellBeingDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<WorkerWellBeingDapperRepository> logger)
    : IWorkerWellBeingRecordRepository
{
    /// <inheritdoc/>
    public async Task<WorkerWellBeingReadModel?> GetByIdAsync(Guid tenantId, Guid workerWellBeingId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get WorkerWellBeing {WorkerWellBeingId} for tenant {TenantId}", workerWellBeingId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("WorkerWellBeingId", workerWellBeingId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<WorkerWellBeingReadModel>(
            "[WorkerWellBeing].[usp_WorkerWellBeing_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<WorkerWellBeingReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? workerId,
        string? wellBeingRiskLevel,
        string? status,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List WorkerWellBeing records for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("WorkerId", workerId)
            .AddString("WellBeingRiskLevel", wellBeingRiskLevel, 20)
            .AddString("Status", status, 50)
            .AddString("OrderBy", orderBy, 100)
            .AddString("SortDirection", sortDirection, 4)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var rows = await queryExecutor.QueryAsync<WorkerWellBeingListRow>(
            "[WorkerWellBeing].[usp_WorkerWellBeing_List]", parameters, cancellationToken);

        var items = rows.Select(r => new WorkerWellBeingReadModel
        {
            WorkerWellBeingId = r.WorkerWellBeingId,
            TenantId = r.TenantId,
            WorkerId = r.WorkerId,
            WellBeingRiskLevel = r.WellBeingRiskLevel,
            Status = r.Status,
            RecordedAtUtc = r.RecordedAtUtc,
        }).ToList();

        var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;
        return (items, totalCount);
    }

    /// <summary>Raw row shape returned by <c>[WorkerWellBeing].[usp_WorkerWellBeing_List]</c>, including the windowed total row count.</summary>
    private sealed record WorkerWellBeingListRow
    {
        public required Guid WorkerWellBeingId { get; init; }
        public required Guid TenantId { get; init; }
        public required Guid WorkerId { get; init; }
        public required string WellBeingRiskLevel { get; init; }
        public required string Status { get; init; }
        public required DateTimeOffset RecordedAtUtc { get; init; }
        public required int TotalCount { get; init; }
    }
}
