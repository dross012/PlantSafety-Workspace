// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           GetWorkflowInstanceQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetWorkflowInstanceQuery.
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

/// <summary>Handles <see cref="GetWorkflowInstanceQuery"/>.</summary>
public sealed class GetWorkflowInstanceQueryHandler : IRequestHandler<GetWorkflowInstanceQuery, Result<WorkflowInstanceDto>>
{
    private readonly IWorkflowInstanceReadRepository _repository;
    private readonly ILogger<GetWorkflowInstanceQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWorkflowInstanceQueryHandler(IWorkflowInstanceReadRepository repository, ILogger<GetWorkflowInstanceQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkflowInstanceDto>> Handle(GetWorkflowInstanceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.WorkflowInstanceId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("WorkflowInstance {WorkflowInstanceId} not found for Tenant {TenantId}", request.WorkflowInstanceId, request.TenantId);

                return Result<WorkflowInstanceDto>.Failure(new ApplicationError(
                    "Workflow.Instance.NotFound",
                    "The requested workflow instance could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<WorkflowInstanceDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve WorkflowInstance {WorkflowInstanceId}", request.WorkflowInstanceId);

            return Result<WorkflowInstanceDto>.Failure(new ApplicationError(
                "Workflow.Instance.RetrievalFailed",
                "Unable to retrieve workflow instance data at this time.",
                Guid.NewGuid()));
        }
    }
}
