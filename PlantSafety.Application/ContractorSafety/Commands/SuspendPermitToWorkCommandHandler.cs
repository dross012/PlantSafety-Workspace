// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for SuspendPermitToWorkCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContractorSafety.PermitToWork.Interfaces;
using PlantSafety.Domain.ContractorSafety.PermitToWork.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContractorSafety.Commands;

/// <summary>Handles <see cref="SuspendPermitToWorkCommand"/>.</summary>
public sealed class SuspendPermitToWorkCommandHandler : IRequestHandler<SuspendPermitToWorkCommand, Result<PermitToWorkActionResult>>
{
    private readonly IPermitToWorkRepository _repository;
    private readonly ILogger<SuspendPermitToWorkCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SuspendPermitToWorkCommandHandler(IPermitToWorkRepository repository, ILogger<SuspendPermitToWorkCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitToWorkActionResult>> Handle(SuspendPermitToWorkCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PermitToWorkId.FromGuid(request.PermitToWorkId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PermitToWorkActionResult>.Failure(new ApplicationError(
                    "ContractorSafety.PermitToWork.NotFound",
                    "The requested PermitToWork record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Suspend(
                request.Reason,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PermitToWorkActionResult>.Success(new PermitToWorkActionResult(
                aggregate.PermitToWorkId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Suspend for PermitToWork {PermitToWorkId}", request.PermitToWorkId);

            return Result<PermitToWorkActionResult>.Failure(new ApplicationError(
                "ContractorSafety.PermitToWork.SuspendFailed",
                "Unable to Suspend the PermitToWork record at this time.",
                Guid.NewGuid()));
        }
    }
}