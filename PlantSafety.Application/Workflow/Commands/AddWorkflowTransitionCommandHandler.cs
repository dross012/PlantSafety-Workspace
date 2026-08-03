// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           AddWorkflowTransitionCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for AddWorkflowTransitionCommand. Loads the target WorkflowDefinition,
//                  adds the transition via the aggregate's own mutator, and persists the change.
//                  No business rules live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.Domain.Workflow.ValueObjects;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Handles <see cref="AddWorkflowTransitionCommand"/>.</summary>
public sealed class AddWorkflowTransitionCommandHandler : IRequestHandler<AddWorkflowTransitionCommand, Result<AddWorkflowTransitionResult>>
{
    private readonly IWorkflowDefinitionRepository _repository;
    private readonly ILogger<AddWorkflowTransitionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AddWorkflowTransitionCommandHandler(IWorkflowDefinitionRepository repository, ILogger<AddWorkflowTransitionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AddWorkflowTransitionResult>> Handle(AddWorkflowTransitionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var definition = await _repository.GetByIdAsync(WorkflowDefinitionId.FromGuid(request.WorkflowDefinitionId), request.TenantId, cancellationToken);
            if (definition is null)
            {
                return Result<AddWorkflowTransitionResult>.Failure(new ApplicationError(
                    "Workflow.Definition.NotFound",
                    "The requested workflow definition could not be found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var guard = string.IsNullOrWhiteSpace(request.GuardField)
                ? GuardCondition.None
                : GuardCondition.Create(
                    request.GuardField,
                    Enum.Parse<GuardOperator>(request.GuardOperatorCode, ignoreCase: true),
                    request.GuardExpectedValue ?? string.Empty);

            definition.AddTransition(
                WorkflowStepId.FromGuid(request.FromStepId),
                WorkflowStepId.FromGuid(request.ToStepId),
                guard,
                request.IsDefault,
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId));

            await _repository.UpdateAsync(definition, eTag: null, cancellationToken);

            return Result<AddWorkflowTransitionResult>.Success(new AddWorkflowTransitionResult(
                definition.WorkflowDefinitionId.Value, request.FromStepId, request.ToStepId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add transition to WorkflowDefinition {WorkflowDefinitionId}", request.WorkflowDefinitionId);

            return Result<AddWorkflowTransitionResult>.Failure(new ApplicationError(
                "Workflow.Definition.AddTransitionFailed",
                "Unable to add the transition to the workflow definition at this time.",
                Guid.NewGuid()));
        }
    }
}
