// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           GetWorkflowHistoryQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetWorkflowHistoryQuery.
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

/// <summary>Handles <see cref="GetWorkflowHistoryQuery"/>.</summary>
public sealed class GetWorkflowHistoryQueryHandler : IRequestHandler<GetWorkflowHistoryQuery, Result<IReadOnlyList<WorkflowStepHistoryEntryDto>>>
{
    private readonly IWorkflowInstanceReadRepository _repository;
    private readonly ILogger<GetWorkflowHistoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWorkflowHistoryQueryHandler(IWorkflowInstanceReadRepository repository, ILogger<GetWorkflowHistoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<WorkflowStepHistoryEntryDto>>> Handle(GetWorkflowHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListHistoryAsync(request.WorkflowInstanceId, request.TenantId, cancellationToken);
            return Result<IReadOnlyList<WorkflowStepHistoryEntryDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve history for WorkflowInstance {WorkflowInstanceId}", request.WorkflowInstanceId);

            return Result<IReadOnlyList<WorkflowStepHistoryEntryDto>>.Failure(new ApplicationError(
                "Workflow.Instance.HistoryRetrievalFailed",
                "Unable to retrieve workflow history at this time.",
                Guid.NewGuid()));
        }
    }
}
