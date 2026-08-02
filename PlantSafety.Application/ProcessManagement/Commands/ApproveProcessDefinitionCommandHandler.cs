// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Application
//  File:           ApproveProcessDefinitionCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ApproveProcessDefinitionCommand. Mirrors
//                  SubmitProcessDefinitionForReviewCommandHandler. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ProcessManagement.Repositories;
using PlantSafety.Domain.ProcessManagement.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ProcessManagement.Commands;

/// <summary>Handles <see cref="ApproveProcessDefinitionCommand"/>.</summary>
public sealed class ApproveProcessDefinitionCommandHandler : IRequestHandler<ApproveProcessDefinitionCommand, Result<ProcessDefinitionStatusResult>>
{
    private readonly IProcessDefinitionRepository _repository;
    private readonly ILogger<ApproveProcessDefinitionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApproveProcessDefinitionCommandHandler(IProcessDefinitionRepository repository, ILogger<ApproveProcessDefinitionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProcessDefinitionStatusResult>> Handle(ApproveProcessDefinitionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ProcessDefinitionId.FromGuid(request.ProcessDefinitionId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ProcessDefinitionStatusResult>.Failure(new ApplicationError(
                    "ProcessManagement.ProcessDefinition.NotFound",
                    "The requested process definition could not be found.",
                    request.ProcessDefinitionId,
                    ErrorSeverity.NotFound));
            }

            var approvedBy = UserId.FromGuid(request.ApprovedByUserId);

            aggregate.Approve(
                approvedBy: approvedBy,
                approvedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(approvedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ProcessDefinitionStatusResult>.Success(new ProcessDefinitionStatusResult(
                aggregate.ProcessDefinitionId.Value, aggregate.Status.ToString(), aggregate.Version));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve ProcessDefinition {ProcessDefinitionId}", request.ProcessDefinitionId);

            return Result<ProcessDefinitionStatusResult>.Failure(new ApplicationError(
                "ProcessManagement.ProcessDefinition.ApproveFailed",
                "Unable to approve the process definition at this time.",
                Guid.NewGuid()));
        }
    }
}
