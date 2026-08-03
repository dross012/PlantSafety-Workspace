// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           GetPendingApprovalsForActorQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve all WorkflowInstances currently waiting on a HumanApproval
//                  step assigned to the given actor (by user id or by role membership).
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Workflow.Models;

namespace PlantSafety.Application.Workflow.Queries;

/// <summary>Retrieves all WorkflowInstances currently pending an approval decision from the given actor.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="UserId">Identity of the actor.</param>
/// <param name="UserRoles">Role codes held by the actor.</param>
public sealed record GetPendingApprovalsForActorQuery(
    Guid TenantId,
    Guid UserId,
    IReadOnlyList<string> UserRoles) : IRequest<Result<IReadOnlyList<WorkflowInstanceDto>>>;
