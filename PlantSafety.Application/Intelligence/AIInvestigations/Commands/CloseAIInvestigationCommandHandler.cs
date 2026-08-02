// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Application
//  File:           CloseAIInvestigationCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseAIInvestigationCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.AIInvestigations.Interfaces;
using PlantSafety.Domain.Intelligence.AIInvestigations.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.AIInvestigations.Commands;

/// <summary>Handles <see cref="CloseAIInvestigationCommand"/>.</summary>
public sealed class CloseAIInvestigationCommandHandler : IRequestHandler<CloseAIInvestigationCommand, Result<AIInvestigationStatusResult>>
{
    private readonly IAIInvestigationRepository _repository;
    private readonly ILogger<CloseAIInvestigationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseAIInvestigationCommandHandler(IAIInvestigationRepository repository, ILogger<CloseAIInvestigationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AIInvestigationStatusResult>> Handle(CloseAIInvestigationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AIInvestigationId.FromGuid(request.AIInvestigationId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<AIInvestigationStatusResult>.Failure(new ApplicationError(
                    "Intelligence.AIInvestigation.NotFound",
                    "The requested AI investigation could not be found.",
                    request.AIInvestigationId,
                    ErrorSeverity.NotFound));
            }

            var closedBy = UserId.FromGuid(request.ClosedByUserId);

            aggregate.Close(
                closedBy: closedBy,
                resolutionSummary: request.ResolutionSummary,
                closedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(closedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<AIInvestigationStatusResult>.Success(new AIInvestigationStatusResult(
                aggregate.AIInvestigationId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close AIInvestigation {AIInvestigationId}", request.AIInvestigationId);

            return Result<AIInvestigationStatusResult>.Failure(new ApplicationError(
                "Intelligence.AIInvestigation.CloseFailed",
                "Unable to close the AI investigation at this time.",
                Guid.NewGuid()));
        }
    }
}
