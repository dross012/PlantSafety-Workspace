// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           DispatchShipmentCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for DispatchShipmentCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Commands;

/// <summary>Handles <see cref="DispatchShipmentCommand"/>.</summary>
public sealed class DispatchShipmentCommandHandler : IRequestHandler<DispatchShipmentCommand, Result<HazmatShipmentStatusResult>>
{
    private readonly IHazmatShipmentRepository _repository;
    private readonly ILogger<DispatchShipmentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DispatchShipmentCommandHandler(IHazmatShipmentRepository repository, ILogger<DispatchShipmentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazmatShipmentStatusResult>> Handle(DispatchShipmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = HazmatShipmentId.FromGuid(request.HazmatShipmentId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<HazmatShipmentStatusResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.ShippingPapers.NotFound",
                    "The requested hazmat shipment could not be found.",
                    request.HazmatShipmentId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Dispatch(
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<HazmatShipmentStatusResult>.Success(new HazmatShipmentStatusResult(
                aggregate.Id.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch HazmatShipment {HazmatShipmentId}", request.HazmatShipmentId);

            return Result<HazmatShipmentStatusResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.ShippingPapers.DispatchFailed",
                "Unable to dispatch the shipment at this time.",
                Guid.NewGuid()));
        }
    }
}
