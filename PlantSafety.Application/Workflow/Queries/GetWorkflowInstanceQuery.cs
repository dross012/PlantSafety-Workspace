// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           GetWorkflowInstanceQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single WorkflowInstance read-model record by identity.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Workflow.Models;

namespace PlantSafety.Application.Workflow.Queries;

/// <summary>Retrieves a single WorkflowInstance read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WorkflowInstanceId">Identity of the WorkflowInstance record to retrieve.</param>
public sealed record GetWorkflowInstanceQuery(
    Guid TenantId,
    Guid WorkflowInstanceId) : IRequest<Result<WorkflowInstanceDto>>;
