// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           MarkShipmentReadyCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for MarkShipmentReadyCommand. The aggregate itself enforces the
//                  "shipping paper attached" and "no Prohibited segregation violations"
//                  invariants (ShipmentNotReadyForDispatchException), which the generic catch
//                  below surfaces as an ApplicationError.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Commands;

/// <summary>Handles <see cref="MarkShipmentReadyCommand"/>.</summary>
public sealed class MarkShipmentReadyCommandHandler : IRequestHandler<MarkShipmentReadyCommand, Result<HazmatShipmentStatusResult>>
{
    private readonly IHazmatShipmentRepository _repository;
    private readonly ILogger<MarkShipmentReadyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public MarkShipmentReadyCommandHandler(IHazmatShipmentRepository repository, ILogger<MarkShipmentReadyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazmatShipmentStatusResult>> Handle(MarkShipmentReadyCommand request, CancellationToken cancellationToken)
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

            aggregate.MarkReady(
                emergencyResponseInfo: new EmergencyResponseInfo(
                    request.ErgGuideNumber,
                    request.EmergencyContactName,
                    request.EmergencyContactPhone,
                    request.ResponseGuidanceSummary),
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
            _logger.LogError(ex, "Failed to mark HazmatShipment {HazmatShipmentId} ready", request.HazmatShipmentId);

            return Result<HazmatShipmentStatusResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.ShippingPapers.MarkReadyFailed",
                "Unable to mark the shipment ready for dispatch at this time.",
                Guid.NewGuid()));
        }
    }
}
