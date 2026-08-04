// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RegisterDischargePointCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Enums;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates.DischargePoint;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Handles <see cref="RegisterDischargePointCommand"/>.</summary>
public sealed class RegisterDischargePointCommandHandler : IRequestHandler<RegisterDischargePointCommand, Result<DischargePointActionResult>>
{
    private readonly IDischargePointRepository _repository;
    private readonly ILogger<RegisterDischargePointCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegisterDischargePointCommandHandler(IDischargePointRepository repository, ILogger<RegisterDischargePointCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DischargePointActionResult>> Handle(RegisterDischargePointCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var outfall = Enum.TryParse<OutfallType>(request.Outfall, true, out var parsedOutfall) ? parsedOutfall : OutfallType.Direct;

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                dischargePermitId: request.DischargePermitId,
                pointCode: request.PointCode,
                pointName: request.PointName,
                receivingWaterBody: request.ReceivingWaterBody,
                outfall: outfall,
                latitude: request.Latitude,
                longitude: request.Longitude);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<DischargePointActionResult>.Success(new DischargePointActionResult(
                aggregate.DischargePointId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register DischargePoint for tenant {TenantId}", request.TenantId);

            return Result<DischargePointActionResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.RegisterFailed",
                "Unable to register the discharge point at this time.",
                Guid.NewGuid()));
        }
    }
}