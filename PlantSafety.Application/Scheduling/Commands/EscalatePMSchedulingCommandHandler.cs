// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for EscalatePMSchedulingCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.PMScheduling.Interfaces;
using PlantSafety.Domain.Scheduling.PMScheduling.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.Commands;

/// <summary>Handles <see cref="EscalatePMSchedulingCommand"/>.</summary>
public sealed class EscalatePMSchedulingCommandHandler : IRequestHandler<EscalatePMSchedulingCommand, Result<PMSchedulingActionResult>>
{
    private readonly IPMSchedulingRepository _repository;
    private readonly ILogger<EscalatePMSchedulingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EscalatePMSchedulingCommandHandler(IPMSchedulingRepository repository, ILogger<EscalatePMSchedulingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PMSchedulingActionResult>> Handle(EscalatePMSchedulingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PMSchedulingId.FromGuid(request.PMSchedulingId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PMSchedulingActionResult>.Failure(new ApplicationError(
                    "Scheduling.PMScheduling.NotFound",
                    "The requested PM scheduling record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Escalate(
                priorityCode: request.PriorityCode,
                escalatedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PMSchedulingActionResult>.Success(new PMSchedulingActionResult(
                aggregate.PMSchedulingId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to escalate PMScheduling {PMSchedulingId}", request.PMSchedulingId);

            return Result<PMSchedulingActionResult>.Failure(new ApplicationError(
                "Scheduling.PMScheduling.EscalateFailed",
                "Unable to escalate the preventive maintenance task at this time.",
                Guid.NewGuid()));
        }
    }
}
