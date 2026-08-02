// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Application
//  File:           ArchiveAiSafetyIntelligenceCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveAiSafetyIntelligenceCommand. Additive-only evolution.
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

/// <summary>Handles <see cref="ArchiveAiSafetyIntelligenceCommand"/>.</summary>
public sealed class ArchiveAiSafetyIntelligenceCommandHandler : IRequestHandler<ArchiveAiSafetyIntelligenceCommand, Result<AiSafetyIntelligenceStatusResult>>
{
    private readonly IAiSafetyIntelligenceRepository _repository;
    private readonly ILogger<ArchiveAiSafetyIntelligenceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveAiSafetyIntelligenceCommandHandler(IAiSafetyIntelligenceRepository repository, ILogger<ArchiveAiSafetyIntelligenceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AiSafetyIntelligenceStatusResult>> Handle(ArchiveAiSafetyIntelligenceCommand request, CancellationToken cancellationToken)
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

            aggregate.Archive(
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
            _logger.LogError(ex, "Failed to archive AiSafetyIntelligence {AiSafetyIntelligenceId}", request.AiSafetyIntelligenceId);

            return Result<AiSafetyIntelligenceStatusResult>.Failure(new ApplicationError(
                "AiSafetyIntelligence.ArchiveFailed",
                "Unable to archive the AI safety intelligence record at this time.",
                Guid.NewGuid()));
        }
    }
}
