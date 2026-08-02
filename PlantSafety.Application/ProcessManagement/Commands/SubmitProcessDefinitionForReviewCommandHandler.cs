// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Application
//  File:           SubmitProcessDefinitionForReviewCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for SubmitProcessDefinitionForReviewCommand. GetByIdAsync is
//                  tenant-scoped, the NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.StartAuditingCommandHandler.
//                  Additive-only evolution.
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

/// <summary>Handles <see cref="SubmitProcessDefinitionForReviewCommand"/>.</summary>
public sealed class SubmitProcessDefinitionForReviewCommandHandler : IRequestHandler<SubmitProcessDefinitionForReviewCommand, Result<ProcessDefinitionStatusResult>>
{
    private readonly IProcessDefinitionRepository _repository;
    private readonly ILogger<SubmitProcessDefinitionForReviewCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SubmitProcessDefinitionForReviewCommandHandler(IProcessDefinitionRepository repository, ILogger<SubmitProcessDefinitionForReviewCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProcessDefinitionStatusResult>> Handle(SubmitProcessDefinitionForReviewCommand request, CancellationToken cancellationToken)
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

            var submittedBy = UserId.FromGuid(request.SubmittedByUserId);

            aggregate.SubmitForReview(
                submittedBy: submittedBy,
                submittedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(submittedBy),
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
            _logger.LogError(ex, "Failed to submit ProcessDefinition {ProcessDefinitionId} for review", request.ProcessDefinitionId);

            return Result<ProcessDefinitionStatusResult>.Failure(new ApplicationError(
                "ProcessManagement.ProcessDefinition.SubmitForReviewFailed",
                "Unable to submit the process definition for review at this time.",
                Guid.NewGuid()));
        }
    }
}
