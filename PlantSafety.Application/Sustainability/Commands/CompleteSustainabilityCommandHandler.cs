// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Application
//  File:           CompleteSustainabilityCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CompleteSustainabilityCommand. Mirrors
//                  ActivateSustainabilityCommandHandler. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Sustainability.Repositories;
using PlantSafety.Domain.Sustainability.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Sustainability.Commands;

/// <summary>Handles <see cref="CompleteSustainabilityCommand"/>.</summary>
public sealed class CompleteSustainabilityCommandHandler : IRequestHandler<CompleteSustainabilityCommand, Result<SustainabilityStatusResult>>
{
    private readonly ISustainabilityRepository _repository;
    private readonly ILogger<CompleteSustainabilityCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteSustainabilityCommandHandler(ISustainabilityRepository repository, ILogger<CompleteSustainabilityCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SustainabilityStatusResult>> Handle(CompleteSustainabilityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = SustainabilityId.FromGuid(request.SustainabilityId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<SustainabilityStatusResult>.Failure(new ApplicationError(
                    "Sustainability.Sustainability.NotFound",
                    "The requested sustainability initiative could not be found.",
                    request.SustainabilityId,
                    ErrorSeverity.NotFound));
            }

            var actorUserId = UserId.FromGuid(request.CompletedByUserId);

            aggregate.Complete(
                actorUserId: actorUserId,
                atUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(actorUserId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<SustainabilityStatusResult>.Success(new SustainabilityStatusResult(
                aggregate.SustainabilityId.Value, aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete Sustainability {SustainabilityId}", request.SustainabilityId);

            return Result<SustainabilityStatusResult>.Failure(new ApplicationError(
                "Sustainability.Sustainability.CompleteFailed",
                "Unable to complete the sustainability initiative at this time.",
                Guid.NewGuid()));
        }
    }
}
