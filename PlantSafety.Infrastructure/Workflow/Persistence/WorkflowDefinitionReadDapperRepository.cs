// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Infrastructure
//  File:           WorkflowDefinitionReadDapperRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of
//                  PlantSafety.Application.Workflow.Repositories.IWorkflowDefinitionReadRepository.
//                  Projects flat rows directly into read-model DTOs -- no domain-aggregate
//                  reconstruction (that is WorkflowDefinitionDapperRepository's job). Mirrors
//                  RiskReadDapperRepository's identical precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Workflow.Models;
using PlantSafety.Application.Workflow.Repositories;

namespace PlantSafety.Infrastructure.Workflow.Persistence;

/// <summary>Dapper-backed read-only repository for the WorkflowDefinition read-model.</summary>
public sealed class WorkflowDefinitionReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<WorkflowDefinitionReadDapperRepository> logger)
    : IWorkflowDefinitionReadRepository
{
    /// <inheritdoc/>
    public async Task<WorkflowDefinitionDto?> GetByIdAsync(Guid workflowDefinitionId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for WorkflowDefinition read-model {WorkflowDefinitionId} Tenant {TenantId}", workflowDefinitionId, tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("WorkflowDefinitionId", workflowDefinitionId).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<DefinitionRow>(
            "[Workflow].[usp_WorkflowDefinition_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        var stepParameters = new StoredProcedureBuilder().AddGuid("WorkflowDefinitionId", workflowDefinitionId).Build();
        var stepRows = await queryExecutor.QueryAsync<StepRow>(
            "[Workflow].[usp_WorkflowStepDefinition_GetByDefinitionId]", stepParameters, cancellationToken);

        var steps = stepRows
            .OrderBy(s => s.Order)
            .Select(s => new WorkflowStepDefinitionDto(s.StepId, s.Order, s.Name, s.StepType, s.AssignedRoleCode, s.AssignedUserId))
            .ToList();

        return new WorkflowDefinitionDto(
            row.WorkflowDefinitionId, row.TenantId, row.SiteId, row.WorkflowType, row.Name, row.Description,
            row.Version, row.IsActive, steps, row.CreatedAtUtc, row.CreatedBy, row.ETag);
    }

    private sealed record DefinitionRow
    {
        public required Guid WorkflowDefinitionId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string WorkflowType { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required int Version { get; init; }
        public required bool IsActive { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public string? ETag { get; init; }
    }

    private sealed record StepRow
    {
        public required Guid StepId { get; init; }
        public required int Order { get; init; }
        public required string Name { get; init; }
        public required string StepType { get; init; }
        public string? AssignedRoleCode { get; init; }
        public Guid? AssignedUserId { get; init; }
    }
}
