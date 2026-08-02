// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Application
//  File:           CloseSustainabilityCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseSustainabilityCommand. Mirrors
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

/// <summary>Handles <see cref="CloseSustainabilityCommand"/>.</summary>
public sealed class CloseSustainabilityCommandHandler : IRequestHandler<CloseSustainabilityCommand, Result<SustainabilityStatusResult>>
{
    private readonly ISustainabilityRepository _repository;
    private readonly ILogger<CloseSustainabilityCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseSustainabilityCommandHandler(ISustainabilityRepository repository, ILogger<CloseSustainabilityCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SustainabilityStatusResult>> Handle(CloseSustainabilityCommand request, CancellationToken cancellationToken)
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

            var actorUserId = UserId.FromGuid(request.ClosedByUserId);

            aggregate.Close(
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
            _logger.LogError(ex, "Failed to close Sustainability {SustainabilityId}", request.SustainabilityId);

            return Result<SustainabilityStatusResult>.Failure(new ApplicationError(
                "Sustainability.Sustainability.CloseFailed",
                "Unable to close the sustainability initiative at this time.",
                Guid.NewGuid()));
        }
    }
}
