// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           GetRouteApprovalWorkflowQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query to project a route definition's approval-workflow state, derived from
//                  its reused ProcessManagement-style lifecycle status. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;

namespace PlantSafety.Application.ProcessManagement.Routing.Queries;

/// <summary>Retrieves the current approval-workflow state for a route definition.</summary>
public sealed record GetRouteApprovalWorkflowQuery(
    Guid TenantId,
    Guid RouteDefinitionId) : IRequest<Result<RouteApprovalWorkflowDto>>;
