// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           GetWorkflowHistoryQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve the full step execution history of a WorkflowInstance, in
//                  activation order.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Workflow.Models;

namespace PlantSafety.Application.Workflow.Queries;

/// <summary>Retrieves the full step execution history of a WorkflowInstance.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WorkflowInstanceId">Identity of the WorkflowInstance whose history is being retrieved.</param>
public sealed record GetWorkflowHistoryQuery(
    Guid TenantId,
    Guid WorkflowInstanceId) : IRequest<Result<IReadOnlyList<WorkflowStepHistoryEntryDto>>>;
