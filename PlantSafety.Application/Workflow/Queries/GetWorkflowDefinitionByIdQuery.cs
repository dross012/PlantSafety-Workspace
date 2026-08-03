// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           GetWorkflowDefinitionByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single WorkflowDefinition read-model record by identity.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Workflow.Models;

namespace PlantSafety.Application.Workflow.Queries;

/// <summary>Retrieves a single WorkflowDefinition read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WorkflowDefinitionId">Identity of the WorkflowDefinition record to retrieve.</param>
public sealed record GetWorkflowDefinitionByIdQuery(
    Guid TenantId,
    Guid WorkflowDefinitionId) : IRequest<Result<WorkflowDefinitionDto>>;
