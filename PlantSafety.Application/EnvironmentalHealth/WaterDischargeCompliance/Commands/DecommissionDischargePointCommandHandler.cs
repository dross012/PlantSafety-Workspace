// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for DecommissionDischargePointCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Handles <see cref="DecommissionDischargePointCommand"/>.</summary>
public sealed class DecommissionDischargePointCommandHandler : IRequestHandler<DecommissionDischargePointCommand, Result<DischargePointActionResult>>
{
    private readonly IDischargePointRepository _repository;
    private readonly ILogger<DecommissionDischargePointCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DecommissionDischargePointCommandHandler(IDischargePointRepository repository, ILogger<DecommissionDischargePointCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DischargePointActionResult>> Handle(DecommissionDischargePointCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = DischargePointId.FromGuid(request.DischargePointId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<DischargePointActionResult>.Failure(new ApplicationError(
                    "EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.NotFound",
                    "The requested DischargePoint record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Decommission(
                request.Reason,
                UtcTimestamp.From(request.RequestedAtUtc),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<DischargePointActionResult>.Success(new DischargePointActionResult(
                aggregate.DischargePointId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Decommission for DischargePoint {DischargePointId}", request.DischargePointId);

            return Result<DischargePointActionResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.DecommissionFailed",
                "Unable to Decommission the DischargePoint record at this time.",
                Guid.NewGuid()));
        }
    }
}