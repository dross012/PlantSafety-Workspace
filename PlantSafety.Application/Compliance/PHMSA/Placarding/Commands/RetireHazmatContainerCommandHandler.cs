// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           RetireHazmatContainerCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for RetireHazmatContainerCommand. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ClosePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

/// <summary>Handles <see cref="RetireHazmatContainerCommand"/>.</summary>
public sealed class RetireHazmatContainerCommandHandler : IRequestHandler<RetireHazmatContainerCommand, Result<HazmatContainerStatusResult>>
{
    private readonly IHazmatContainerRepository _repository;
    private readonly ILogger<RetireHazmatContainerCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RetireHazmatContainerCommandHandler(IHazmatContainerRepository repository, ILogger<RetireHazmatContainerCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazmatContainerStatusResult>> Handle(RetireHazmatContainerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = HazmatContainerId.FromGuid(request.HazmatContainerId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<HazmatContainerStatusResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Placarding.NotFound",
                    "The requested hazmat container could not be found.",
                    request.HazmatContainerId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Retire(
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<HazmatContainerStatusResult>.Success(new HazmatContainerStatusResult(
                aggregate.Id.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retire HazmatContainer {HazmatContainerId}", request.HazmatContainerId);

            return Result<HazmatContainerStatusResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Placarding.RetireFailed",
                "Unable to retire the hazmat container at this time.",
                Guid.NewGuid()));
        }
    }
}
