// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Application
//  File:           CloseAiSafetyIntelligenceCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseAiSafetyIntelligenceCommand. Additive-only evolution.
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

/// <summary>Handles <see cref="CloseAiSafetyIntelligenceCommand"/>.</summary>
public sealed class CloseAiSafetyIntelligenceCommandHandler : IRequestHandler<CloseAiSafetyIntelligenceCommand, Result<AiSafetyIntelligenceStatusResult>>
{
    private readonly IAiSafetyIntelligenceRepository _repository;
    private readonly ILogger<CloseAiSafetyIntelligenceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseAiSafetyIntelligenceCommandHandler(IAiSafetyIntelligenceRepository repository, ILogger<CloseAiSafetyIntelligenceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AiSafetyIntelligenceStatusResult>> Handle(CloseAiSafetyIntelligenceCommand request, CancellationToken cancellationToken)
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

            aggregate.Close(
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
            _logger.LogError(ex, "Failed to close AiSafetyIntelligence {AiSafetyIntelligenceId}", request.AiSafetyIntelligenceId);

            return Result<AiSafetyIntelligenceStatusResult>.Failure(new ApplicationError(
                "AiSafetyIntelligence.CloseFailed",
                "Unable to close the AI safety intelligence record at this time.",
                Guid.NewGuid()));
        }
    }
}
