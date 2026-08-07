// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordLoadSecurementCheckCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck;

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Handles <see cref="RecordLoadSecurementCheckCommand"/>.</summary>
public sealed class RecordLoadSecurementCheckCommandHandler : IRequestHandler<RecordLoadSecurementCheckCommand, Result<LoadSecurementCheckActionResult>>
{
    private readonly ILoadSecurementCheckRepository _repository;
    private readonly ILogger<RecordLoadSecurementCheckCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordLoadSecurementCheckCommandHandler(ILoadSecurementCheckRepository repository, ILogger<RecordLoadSecurementCheckCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LoadSecurementCheckActionResult>> Handle(RecordLoadSecurementCheckCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                tripId: request.TripId,
                vehicleId: request.VehicleId,
                checkedByUserId: request.CheckedByUserId,
                passed: request.Passed,
                deficiencyCount: request.DeficiencyCount,
                securementScore: request.SecurementScore);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<LoadSecurementCheckActionResult>.Success(new LoadSecurementCheckActionResult(
                aggregate.LoadSecurementCheckId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record LoadSecurementCheck for Tenant {TenantId}", request.TenantId);

            return Result<LoadSecurementCheckActionResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.LoadSecurementChecks.RecordFailed",
                "Unable to record the LoadSecurementCheck at this time.",
                Guid.NewGuid()));
        }
    }
}
