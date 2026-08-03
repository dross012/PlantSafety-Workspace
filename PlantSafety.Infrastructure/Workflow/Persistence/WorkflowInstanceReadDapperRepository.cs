// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Infrastructure
//  File:           WorkflowInstanceReadDapperRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of
//                  PlantSafety.Application.Workflow.Repositories.IWorkflowInstanceReadRepository.
//                  Projects flat rows directly into read-model DTOs -- no domain-aggregate
//                  reconstruction (that is WorkflowInstanceDapperRepository's job). Mirrors
//                  RiskReadDapperRepository's identical precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Workflow.Models;
using PlantSafety.Application.Workflow.Repositories;

namespace PlantSafety.Infrastructure.Workflow.Persistence;

/// <summary>Dapper-backed read-only repository for the WorkflowInstance read-model.</summary>
public sealed class WorkflowInstanceReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<WorkflowInstanceReadDapperRepository> logger)
    : IWorkflowInstanceReadRepository
{
    /// <inheritdoc/>
    public async Task<WorkflowInstanceDto?> GetByIdAsync(Guid workflowInstanceId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for WorkflowInstance read-model {WorkflowInstanceId} Tenant {TenantId}", workflowInstanceId, tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("WorkflowInstanceId", workflowInstanceId).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<InstanceRow>(
            "[Workflow].[usp_WorkflowInstance_GetById]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkflowInstanceDto>> ListPendingApprovalsForActorAsync(
        Guid tenantId, Guid userId, IReadOnlyList<string> userRoles, CancellationToken cancellationToken)
    {
        logger.LogDebug("ListPendingApprovalsForActor for User {UserId} Tenant {TenantId}", userId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("UserId", userId)
            .AddString("UserRolesCsv", userRoles.Count > 0 ? string.Join(',', userRoles) : null, size: -1)
            .Build();

        var rows = await queryExecutor.QueryAsync<InstanceRow>(
            "[Workflow].[usp_WorkflowInstance_GetPendingApprovalsForActor]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkflowStepHistoryEntryDto>> ListHistoryAsync(Guid workflowInstanceId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("ListHistory for WorkflowInstance {WorkflowInstanceId} Tenant {TenantId}", workflowInstanceId, tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("WorkflowInstanceId", workflowInstanceId).Build();
        var rows = await queryExecutor.QueryAsync<StepHistoryRow>(
            "[Workflow].[usp_WorkflowStepInstance_GetByInstanceId]", parameters, cancellationToken);

        return rows
            .OrderBy(r => r.SequenceNo)
            .Select(r => new WorkflowStepHistoryEntryDto(
                r.StepInstanceId, r.StepId, r.StepType, r.Status, r.AssignedRoleCode, r.AssignedUserId,
                r.ActivatedAtUtc, r.DecidedAtUtc, r.DecidedBy, r.DecisionNotes, r.AttemptCount, r.NextAttemptAtUtc, r.RecommendationReference))
            .ToList();
    }

    private static WorkflowInstanceDto ToDto(InstanceRow row)
        => new(row.WorkflowInstanceId, row.WorkflowDefinitionId, row.TenantId, row.SiteId, row.WorkflowType, row.State,
            row.CurrentStepId, row.StartedAtUtc, row.CompletedAtUtc, row.InitiatedBy, row.ETag);

    private sealed record InstanceRow
    {
        public required Guid WorkflowInstanceId { get; init; }
        public required Guid WorkflowDefinitionId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string WorkflowType { get; init; }
        public required string State { get; init; }
        public Guid? CurrentStepId { get; init; }
        public string? ContextDataJson { get; init; }
        public required DateTime StartedAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public required Guid InitiatedBy { get; init; }
        public Guid CorrelationId { get; init; }
        public string? ETag { get; init; }
    }

    private sealed record StepHistoryRow
    {
        public required Guid StepInstanceId { get; init; }
        public required Guid WorkflowInstanceId { get; init; }
        public required Guid StepId { get; init; }
        public required string StepType { get; init; }
        public required string Status { get; init; }
        public string? AssignedRoleCode { get; init; }
        public Guid? AssignedUserId { get; init; }
        public DateTime? ActivatedAtUtc { get; init; }
        public DateTime? DecidedAtUtc { get; init; }
        public Guid? DecidedBy { get; init; }
        public string? DecisionNotes { get; init; }
        public required int AttemptCount { get; init; }
        public DateTime? NextAttemptAtUtc { get; init; }
        public string? RecommendationReference { get; init; }
        public required int SequenceNo { get; init; }
    }
}
