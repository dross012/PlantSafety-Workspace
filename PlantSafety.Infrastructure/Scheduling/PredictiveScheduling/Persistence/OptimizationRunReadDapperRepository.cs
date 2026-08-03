// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Infrastructure
//  File:           OptimizationRunReadDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the OptimizationRun Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Scheduling.PredictiveScheduling.Models;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Repositories;

namespace PlantSafety.Infrastructure.Scheduling.PredictiveScheduling.Persistence;

/// <summary>Dapper-backed repository for OptimizationRun read-model queries.</summary>
public sealed class OptimizationRunReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<OptimizationRunReadDapperRepository> logger)
    : IOptimizationRunReadRepository
{
    /// <inheritdoc/>
    public async Task<OptimizationRunDto?> GetByIdAsync(Guid optimizationRunId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for OptimizationRun {OptimizationRunId} Tenant {TenantId}", optimizationRunId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("OptimizationRunId", optimizationRunId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<OptimizationRunReadRow>(
            "[Scheduling].[usp_OptimizationRun_GetById]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OptimizationRunSummaryDto>> ListHistoryAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime fromUtc,
        DateTime toUtc,
        string? statusCode,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List OptimizationRun history for Tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", fromUtc)
            .AddDateTime("ToUtc", toUtc)
            .AddString("StatusCode", statusCode, 20)
            .Build();

        var rows = await queryExecutor.QueryAsync<OptimizationRunReadRow>(
            "[Scheduling].[usp_OptimizationRun_List]", parameters, cancellationToken);

        return rows.Select(r => new OptimizationRunSummaryDto(
            r.OptimizationRunId, r.Objective, r.Status, r.SelectedStrategyName, r.Score,
            r.AssignmentCount, r.UnscheduledTaskCount, r.RequestedAtUtc, r.CompletedAtUtc)).ToList();
    }

    private static OptimizationRunDto ToDto(OptimizationRunReadRow row) => new(
        OptimizationRunId: row.OptimizationRunId,
        TenantId: row.TenantId,
        SiteId: row.SiteId,
        Objective: row.Objective,
        Status: row.Status,
        SelectedStrategyName: row.SelectedStrategyName,
        Score: row.Score,
        InputTaskCount: row.InputTaskCount,
        InputAvailabilityCount: row.InputAvailabilityCount,
        Assignments: (JsonSerializer.Deserialize<List<ScheduleAssignmentJsonRow>>(row.ResultAssignmentsJson) ?? [])
            .Select(a => new ScheduleAssignmentDto(a.TaskId, a.ResourceId, a.StartUtc, a.EndUtc, a.EffectivePriorityWeight)).ToList(),
        UnscheduledTaskIds: JsonSerializer.Deserialize<List<Guid>>(row.UnscheduledTaskIdsJson) ?? [],
        ToleratedViolations: (JsonSerializer.Deserialize<List<ConstraintViolationJsonRow>>(row.ToleratedViolationsJson) ?? [])
            .Select(v => new ConstraintViolationDto(v.RuleName, v.Severity, v.Message, v.TaskId)).ToList(),
        FailureReason: row.FailureReason,
        RequestedAtUtc: row.RequestedAtUtc,
        CompletedAtUtc: row.CompletedAtUtc,
        RequestedBy: row.RequestedBy,
        CorrelationId: row.CorrelationId);

    private sealed record ScheduleAssignmentJsonRow(Guid TaskId, Guid ResourceId, DateTime StartUtc, DateTime EndUtc, double EffectivePriorityWeight);

    private sealed record ConstraintViolationJsonRow(string RuleName, string Severity, string Message, Guid TaskId);

    /// <summary>Flat read-model row shape returned by usp_OptimizationRun_GetById/usp_OptimizationRun_List.</summary>
    private sealed record OptimizationRunReadRow
    {
        public required Guid OptimizationRunId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Objective { get; init; }
        public required string Status { get; init; }
        public string? SelectedStrategyName { get; init; }
        public double? Score { get; init; }
        public string? FailureReason { get; init; }
        public int InputTaskCount { get; init; }
        public int InputAvailabilityCount { get; init; }
        public int AssignmentCount { get; init; }
        public int UnscheduledTaskCount { get; init; }
        public int ToleratedViolationCount { get; init; }
        public required string ResultAssignmentsJson { get; init; }
        public required string UnscheduledTaskIdsJson { get; init; }
        public required string ToleratedViolationsJson { get; init; }
        public required DateTime RequestedAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public Guid? RequestedBy { get; init; }
        public required Guid CorrelationId { get; init; }
        public string? ETag { get; init; }
    }
}
