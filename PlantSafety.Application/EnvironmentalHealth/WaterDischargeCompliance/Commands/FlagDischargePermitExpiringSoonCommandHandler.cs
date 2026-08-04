// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for FlagDischargePermitExpiringSoonCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Handles <see cref="FlagDischargePermitExpiringSoonCommand"/>.</summary>
public sealed class FlagDischargePermitExpiringSoonCommandHandler : IRequestHandler<FlagDischargePermitExpiringSoonCommand, Result<DischargePermitActionResult>>
{
    private readonly IDischargePermitRepository _repository;
    private readonly ILogger<FlagDischargePermitExpiringSoonCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public FlagDischargePermitExpiringSoonCommandHandler(IDischargePermitRepository repository, ILogger<FlagDischargePermitExpiringSoonCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DischargePermitActionResult>> Handle(FlagDischargePermitExpiringSoonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = DischargePermitId.FromGuid(request.DischargePermitId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<DischargePermitActionResult>.Failure(new ApplicationError(
                    "EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.NotFound",
                    "The requested DischargePermit record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.FlagExpiringSoon(
                UtcTimestamp.From(request.RequestedAtUtc),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<DischargePermitActionResult>.Success(new DischargePermitActionResult(
                aggregate.DischargePermitId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute FlagExpiringSoon for DischargePermit {DischargePermitId}", request.DischargePermitId);

            return Result<DischargePermitActionResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.FlagExpiringSoonFailed",
                "Unable to FlagExpiringSoon the DischargePermit record at this time.",
                Guid.NewGuid()));
        }
    }
}