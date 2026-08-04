// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for AcknowledgeAccessAnomalyCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Handles <see cref="AcknowledgeAccessAnomalyCommand"/>.</summary>
public sealed class AcknowledgeAccessAnomalyCommandHandler : IRequestHandler<AcknowledgeAccessAnomalyCommand, Result<AccessAnomalyActionResult>>
{
    private readonly IAccessAnomalyRepository _repository;
    private readonly ILogger<AcknowledgeAccessAnomalyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AcknowledgeAccessAnomalyCommandHandler(IAccessAnomalyRepository repository, ILogger<AcknowledgeAccessAnomalyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AccessAnomalyActionResult>> Handle(AcknowledgeAccessAnomalyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AccessAnomalyId.FromGuid(request.AccessAnomalyId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<AccessAnomalyActionResult>.Failure(new ApplicationError(
                    "PhysicalAccessControl.AccessAnomaly.NotFound",
                    "The requested AccessAnomaly record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Acknowledge(
                request.AcknowledgedByUserId,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<AccessAnomalyActionResult>.Success(new AccessAnomalyActionResult(
                aggregate.AccessAnomalyId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Acknowledge for AccessAnomaly {AccessAnomalyId}", request.AccessAnomalyId);

            return Result<AccessAnomalyActionResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.AccessAnomaly.AcknowledgeFailed",
                "Unable to acknowledge the AccessAnomaly record at this time.",
                Guid.NewGuid()));
        }
    }
}
