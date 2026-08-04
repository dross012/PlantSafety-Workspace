// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for VoidEffluentSampleCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Handles <see cref="VoidEffluentSampleCommand"/>.</summary>
public sealed class VoidEffluentSampleCommandHandler : IRequestHandler<VoidEffluentSampleCommand, Result<EffluentSampleActionResult>>
{
    private readonly IEffluentSampleRepository _repository;
    private readonly ILogger<VoidEffluentSampleCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public VoidEffluentSampleCommandHandler(IEffluentSampleRepository repository, ILogger<VoidEffluentSampleCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EffluentSampleActionResult>> Handle(VoidEffluentSampleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = EffluentSampleId.FromGuid(request.EffluentSampleId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<EffluentSampleActionResult>.Failure(new ApplicationError(
                    "EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.NotFound",
                    "The requested EffluentSample record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Void(
                request.Reason,
                UtcTimestamp.From(request.RequestedAtUtc),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<EffluentSampleActionResult>.Success(new EffluentSampleActionResult(
                aggregate.EffluentSampleId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Void for EffluentSample {EffluentSampleId}", request.EffluentSampleId);

            return Result<EffluentSampleActionResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.VoidFailed",
                "Unable to Void the EffluentSample record at this time.",
                Guid.NewGuid()));
        }
    }
}