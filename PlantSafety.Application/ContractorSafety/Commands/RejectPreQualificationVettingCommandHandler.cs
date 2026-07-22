// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RejectPreQualificationVettingCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Interfaces;
using PlantSafety.Domain.ContractorSafety.PreQualificationVetting.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContractorSafety.Commands;

/// <summary>Handles <see cref="RejectPreQualificationVettingCommand"/>.</summary>
public sealed class RejectPreQualificationVettingCommandHandler : IRequestHandler<RejectPreQualificationVettingCommand, Result<PreQualificationVettingActionResult>>
{
    private readonly IPreQualificationVettingRepository _repository;
    private readonly ILogger<RejectPreQualificationVettingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RejectPreQualificationVettingCommandHandler(IPreQualificationVettingRepository repository, ILogger<RejectPreQualificationVettingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PreQualificationVettingActionResult>> Handle(RejectPreQualificationVettingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PreQualificationVettingId.FromGuid(request.PreQualificationVettingId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PreQualificationVettingActionResult>.Failure(new ApplicationError(
                    "ContractorSafety.PreQualificationVetting.NotFound",
                    "The requested PreQualificationVetting record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Reject(
                request.Reason,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PreQualificationVettingActionResult>.Success(new PreQualificationVettingActionResult(
                aggregate.PreQualificationVettingId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Reject for PreQualificationVetting {PreQualificationVettingId}", request.PreQualificationVettingId);

            return Result<PreQualificationVettingActionResult>.Failure(new ApplicationError(
                "ContractorSafety.PreQualificationVetting.RejectFailed",
                "Unable to Reject the PreQualificationVetting record at this time.",
                Guid.NewGuid()));
        }
    }
}