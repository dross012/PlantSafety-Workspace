// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           GetRouteApprovalWorkflowQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for GetRouteApprovalWorkflowQuery. Derives the workflow view purely
//                  from the reused ProcessManagement-style lifecycle Status: a single-stage
//                  workflow whose sole approver is the route's accountable owner.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;
using PlantSafety.Application.ProcessManagement.Routing.Queries;
using PlantSafety.Application.ProcessManagement.Routing.Repositories;

namespace PlantSafety.Application.ProcessManagement.Routing.Handlers;

/// <summary>Handles <see cref="GetRouteApprovalWorkflowQuery"/>.</summary>
public sealed class GetRouteApprovalWorkflowQueryHandler : IRequestHandler<GetRouteApprovalWorkflowQuery, Result<RouteApprovalWorkflowDto>>
{
    private readonly IRouteDefinitionReadRepository _repository;
    private readonly ILogger<GetRouteApprovalWorkflowQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRouteApprovalWorkflowQueryHandler(IRouteDefinitionReadRepository repository, ILogger<GetRouteApprovalWorkflowQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RouteApprovalWorkflowDto>> Handle(GetRouteApprovalWorkflowQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var route = await _repository.GetByIdAsync(request.TenantId, request.RouteDefinitionId, cancellationToken);
            if (route is null)
            {
                return Result<RouteApprovalWorkflowDto>.Failure(new ApplicationError(
                    "ProcessManagement.Routing.RouteDefinition.NotFound",
                    "The requested route definition could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var (currentStage, approverStatus, decidedAtUtc, canCurrentUserAct) = route.Status switch
            {
                "Draft" => ("Draft", "Pending", (DateTime?)null, false),
                "UnderReview" => ("Review", "Pending", null, true),
                "Active" => ("Approved", "Approved", route.LastAuditedUtc, false),
                "Retired" => ("Retired", "Approved", route.LastAuditedUtc, false),
                "Archived" => ("Archived", "Approved", route.LastAuditedUtc, false),
                _ => (route.Status, "Pending", (DateTime?)null, false),
            };

            var dto = new RouteApprovalWorkflowDto
            {
                CurrentStage = currentStage,
                Approvers =
                [
                    new RouteApproverDto
                    {
                        ApproverName = route.Owner,
                        Role = "Owner",
                        Status = approverStatus,
                        DecidedAtUtc = decidedAtUtc,
                    }
                ],
                CanCurrentUserAct = canCurrentUserAct,
            };

            return Result<RouteApprovalWorkflowDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve approval workflow for RouteDefinition {RouteDefinitionId}", request.RouteDefinitionId);

            return Result<RouteApprovalWorkflowDto>.Failure(new ApplicationError(
                "ProcessManagement.Routing.RouteDefinition.ApprovalWorkflowRetrievalFailed",
                "Unable to retrieve the route's approval workflow at this time.",
                Guid.NewGuid()));
        }
    }
}
