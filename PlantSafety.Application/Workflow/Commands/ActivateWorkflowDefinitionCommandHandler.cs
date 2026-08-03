// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           ActivateWorkflowDefinitionCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ActivateWorkflowDefinitionCommand. Pure orchestration.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Handles <see cref="ActivateWorkflowDefinitionCommand"/>.</summary>
public sealed class ActivateWorkflowDefinitionCommandHandler : IRequestHandler<ActivateWorkflowDefinitionCommand, Result<CreateWorkflowDefinitionResult>>
{
    private readonly IWorkflowDefinitionRepository _repository;
    private readonly ILogger<ActivateWorkflowDefinitionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ActivateWorkflowDefinitionCommandHandler(IWorkflowDefinitionRepository repository, ILogger<ActivateWorkflowDefinitionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateWorkflowDefinitionResult>> Handle(ActivateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var definition = await _repository.GetByIdAsync(WorkflowDefinitionId.FromGuid(request.WorkflowDefinitionId), request.TenantId, cancellationToken);
            if (definition is null)
            {
                return Result<CreateWorkflowDefinitionResult>.Failure(new ApplicationError(
                    "Workflow.Definition.NotFound",
                    "The requested workflow definition could not be found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            definition.Activate(
                UtcTimestamp.Now(),
                PerformedByActor.User(requesterId),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId));

            await _repository.UpdateAsync(definition, eTag: null, cancellationToken);

            return Result<CreateWorkflowDefinitionResult>.Success(new CreateWorkflowDefinitionResult(
                definition.WorkflowDefinitionId.Value, definition.IsActive));
        }
        catch (InvalidOperationException ex)
        {
            return Result<CreateWorkflowDefinitionResult>.Failure(new ApplicationError(
                "Workflow.Definition.ActivateFailed",
                ex.Message,
                request.CorrelationId,
                ErrorSeverity.Domain));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate WorkflowDefinition {WorkflowDefinitionId}", request.WorkflowDefinitionId);

            return Result<CreateWorkflowDefinitionResult>.Failure(new ApplicationError(
                "Workflow.Definition.ActivateFailed",
                "Unable to activate the workflow definition at this time.",
                Guid.NewGuid()));
        }
    }
}
