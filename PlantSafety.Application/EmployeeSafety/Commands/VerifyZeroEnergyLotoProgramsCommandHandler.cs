// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for VerifyZeroEnergyLotoProgramsCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EmployeeSafety.LotoPrograms.Interfaces;
using PlantSafety.Domain.EmployeeSafety.LotoPrograms.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Handles <see cref="VerifyZeroEnergyLotoProgramsCommand"/>.</summary>
public sealed class VerifyZeroEnergyLotoProgramsCommandHandler : IRequestHandler<VerifyZeroEnergyLotoProgramsCommand, Result<LotoProgramsActionResult>>
{
    private readonly ILotoProgramsRepository _repository;
    private readonly ILogger<VerifyZeroEnergyLotoProgramsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public VerifyZeroEnergyLotoProgramsCommandHandler(ILotoProgramsRepository repository, ILogger<VerifyZeroEnergyLotoProgramsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LotoProgramsActionResult>> Handle(VerifyZeroEnergyLotoProgramsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = LotoProgramsId.FromGuid(request.LotoProgramsId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<LotoProgramsActionResult>.Failure(new ApplicationError(
                    "EmployeeSafety.LotoPrograms.NotFound",
                    "The requested LotoPrograms record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.VerifyZeroEnergy(
                UserId.FromGuid(request.VerifiedByUserId),
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<LotoProgramsActionResult>.Success(new LotoProgramsActionResult(
                aggregate.LotoProgramsId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute VerifyZeroEnergy for LotoPrograms {LotoProgramsId}", request.LotoProgramsId);

            return Result<LotoProgramsActionResult>.Failure(new ApplicationError(
                "EmployeeSafety.LotoPrograms.VerifyZeroEnergyFailed",
                "Unable to VerifyZeroEnergy the LotoPrograms record at this time.",
                Guid.NewGuid()));
        }
    }
}