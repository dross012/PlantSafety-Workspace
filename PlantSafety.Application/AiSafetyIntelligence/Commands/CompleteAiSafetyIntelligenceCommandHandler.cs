// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Application
//  File:           CompleteAiSafetyIntelligenceCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CompleteAiSafetyIntelligenceCommand. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AiSafetyIntelligence.Repositories;
using PlantSafety.Domain.AiSafetyIntelligence.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AiSafetyIntelligence.Commands;

/// <summary>Handles <see cref="CompleteAiSafetyIntelligenceCommand"/>.</summary>
public sealed class CompleteAiSafetyIntelligenceCommandHandler : IRequestHandler<CompleteAiSafetyIntelligenceCommand, Result<AiSafetyIntelligenceStatusResult>>
{
    private readonly IAiSafetyIntelligenceRepository _repository;
    private readonly ILogger<CompleteAiSafetyIntelligenceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteAiSafetyIntelligenceCommandHandler(IAiSafetyIntelligenceRepository repository, ILogger<CompleteAiSafetyIntelligenceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AiSafetyIntelligenceStatusResult>> Handle(CompleteAiSafetyIntelligenceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AiSafetyIntelligenceId.FromGuid(request.AiSafetyIntelligenceId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<AiSafetyIntelligenceStatusResult>.Failure(new ApplicationError(
                    "AiSafetyIntelligence.NotFound",
                    "The requested AI safety intelligence record could not be found.",
                    request.AiSafetyIntelligenceId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Complete(
                actorUserId: performedBy,
                atUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<AiSafetyIntelligenceStatusResult>.Success(new AiSafetyIntelligenceStatusResult(
                aggregate.AiSafetyIntelligenceId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete AiSafetyIntelligence {AiSafetyIntelligenceId}", request.AiSafetyIntelligenceId);

            return Result<AiSafetyIntelligenceStatusResult>.Failure(new ApplicationError(
                "AiSafetyIntelligence.CompleteFailed",
                "Unable to complete the AI safety intelligence record at this time.",
                Guid.NewGuid()));
        }
    }
}
