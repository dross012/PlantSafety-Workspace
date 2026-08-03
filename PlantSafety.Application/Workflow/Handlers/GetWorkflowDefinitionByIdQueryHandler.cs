// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           GetWorkflowDefinitionByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetWorkflowDefinitionByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound
//                  failure both when the row does not exist and when it belongs to a different
//                  tenant, so tenant membership is never revealed to an unauthorized caller.
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

/// <summary>Handles <see cref="GetWorkflowDefinitionByIdQuery"/>.</summary>
public sealed class GetWorkflowDefinitionByIdQueryHandler : IRequestHandler<GetWorkflowDefinitionByIdQuery, Result<WorkflowDefinitionDto>>
{
    private readonly IWorkflowDefinitionReadRepository _repository;
    private readonly ILogger<GetWorkflowDefinitionByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWorkflowDefinitionByIdQueryHandler(IWorkflowDefinitionReadRepository repository, ILogger<GetWorkflowDefinitionByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkflowDefinitionDto>> Handle(GetWorkflowDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.WorkflowDefinitionId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("WorkflowDefinition {WorkflowDefinitionId} not found for Tenant {TenantId}", request.WorkflowDefinitionId, request.TenantId);

                return Result<WorkflowDefinitionDto>.Failure(new ApplicationError(
                    "Workflow.Definition.NotFound",
                    "The requested workflow definition could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<WorkflowDefinitionDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve WorkflowDefinition {WorkflowDefinitionId}", request.WorkflowDefinitionId);

            return Result<WorkflowDefinitionDto>.Failure(new ApplicationError(
                "Workflow.Definition.RetrievalFailed",
                "Unable to retrieve workflow definition data at this time.",
                Guid.NewGuid()));
        }
    }
}
