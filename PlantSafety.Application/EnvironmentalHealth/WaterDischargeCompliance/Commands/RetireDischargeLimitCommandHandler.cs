// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RetireDischargeLimitCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Handles <see cref="RetireDischargeLimitCommand"/>.</summary>
public sealed class RetireDischargeLimitCommandHandler : IRequestHandler<RetireDischargeLimitCommand, Result<DischargeLimitActionResult>>
{
    private readonly IDischargeLimitRepository _repository;
    private readonly ILogger<RetireDischargeLimitCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RetireDischargeLimitCommandHandler(IDischargeLimitRepository repository, ILogger<RetireDischargeLimitCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DischargeLimitActionResult>> Handle(RetireDischargeLimitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = DischargeLimitId.FromGuid(request.DischargeLimitId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<DischargeLimitActionResult>.Failure(new ApplicationError(
                    "EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.NotFound",
                    "The requested DischargeLimit record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Retire(
                request.Reason,
                UtcTimestamp.From(request.RequestedAtUtc),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<DischargeLimitActionResult>.Success(new DischargeLimitActionResult(
                aggregate.DischargeLimitId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Retire for DischargeLimit {DischargeLimitId}", request.DischargeLimitId);

            return Result<DischargeLimitActionResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.RetireFailed",
                "Unable to Retire the DischargeLimit record at this time.",
                Guid.NewGuid()));
        }
    }
}