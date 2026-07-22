// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for IssuePermitToWorkCommand.
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

/// <summary>Handles <see cref="IssuePermitToWorkCommand"/>.</summary>
public sealed class IssuePermitToWorkCommandHandler : IRequestHandler<IssuePermitToWorkCommand, Result<PermitToWorkActionResult>>
{
    private readonly IPermitToWorkRepository _repository;
    private readonly ILogger<IssuePermitToWorkCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public IssuePermitToWorkCommandHandler(IPermitToWorkRepository repository, ILogger<IssuePermitToWorkCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitToWorkActionResult>> Handle(IssuePermitToWorkCommand request, CancellationToken cancellationToken)
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

            aggregate.Issue(
                UserId.FromGuid(request.AuthorizedByUserId),
                UtcTimestamp.Now(),
                request.ValidityHours,
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PermitToWorkActionResult>.Success(new PermitToWorkActionResult(
                aggregate.PermitToWorkId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Issue for PermitToWork {PermitToWorkId}", request.PermitToWorkId);

            return Result<PermitToWorkActionResult>.Failure(new ApplicationError(
                "ContractorSafety.PermitToWork.IssueFailed",
                "Unable to Issue the PermitToWork record at this time.",
                Guid.NewGuid()));
        }
    }
}