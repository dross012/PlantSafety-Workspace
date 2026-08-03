// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           GetPendingApprovalsForActorQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetPendingApprovalsForActorQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Workflow.Models;
using PlantSafety.Application.Workflow.Queries;
using PlantSafety.Application.Workflow.Repositories;

namespace PlantSafety.Application.Workflow.Handlers;

/// <summary>Handles <see cref="GetPendingApprovalsForActorQuery"/>.</summary>
public sealed class GetPendingApprovalsForActorQueryHandler : IRequestHandler<GetPendingApprovalsForActorQuery, Result<IReadOnlyList<WorkflowInstanceDto>>>
{
    private readonly IWorkflowInstanceReadRepository _repository;
    private readonly ILogger<GetPendingApprovalsForActorQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPendingApprovalsForActorQueryHandler(IWorkflowInstanceReadRepository repository, ILogger<GetPendingApprovalsForActorQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<WorkflowInstanceDto>>> Handle(GetPendingApprovalsForActorQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListPendingApprovalsForActorAsync(request.TenantId, request.UserId, request.UserRoles, cancellationToken);
            return Result<IReadOnlyList<WorkflowInstanceDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve pending approvals for actor {UserId}", request.UserId);

            return Result<IReadOnlyList<WorkflowInstanceDto>>.Failure(new ApplicationError(
                "Workflow.Instance.PendingApprovalsRetrievalFailed",
                "Unable to retrieve pending approvals at this time.",
                Guid.NewGuid()));
        }
    }
}
