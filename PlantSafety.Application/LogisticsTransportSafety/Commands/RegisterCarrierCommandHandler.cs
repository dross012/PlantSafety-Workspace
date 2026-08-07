// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RegisterCarrierCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.LogisticsTransportSafety.Carriers.Enums;
using PlantSafety.Domain.LogisticsTransportSafety.Carriers.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier;

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Handles <see cref="RegisterCarrierCommand"/>.</summary>
public sealed class RegisterCarrierCommandHandler : IRequestHandler<RegisterCarrierCommand, Result<CarrierActionResult>>
{
    private readonly ICarrierRepository _repository;
    private readonly ILogger<RegisterCarrierCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegisterCarrierCommandHandler(ICarrierRepository repository, ILogger<RegisterCarrierCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CarrierActionResult>> Handle(RegisterCarrierCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            if (!Enum.TryParse<CarrierType>(request.CarrierType, ignoreCase: true, out var carrierType))
            {
                return Result<CarrierActionResult>.Failure(new ApplicationError(
                    "LogisticsTransportSafety.Carriers.InvalidCarrierType",
                    $"'{request.CarrierType}' is not a recognised carrier type.",
                    Guid.NewGuid()));
            }

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                carrierName: request.CarrierName,
                carrierType: carrierType,
                insuranceExpiryAtUtc: request.InsuranceExpiryAtUtc.HasValue ? UtcTimestamp.From(request.InsuranceExpiryAtUtc.Value) : null);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CarrierActionResult>.Success(new CarrierActionResult(
                aggregate.CarrierId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register Carrier for Tenant {TenantId}", request.TenantId);

            return Result<CarrierActionResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.Carriers.RegisterFailed",
                "Unable to register the Carrier record at this time.",
                Guid.NewGuid()));
        }
    }
}
