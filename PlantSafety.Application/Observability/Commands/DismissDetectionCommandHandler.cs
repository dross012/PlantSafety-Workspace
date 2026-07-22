// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for DismissDetectionCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Observability.AutomatedIncidentDetection.Interfaces;
using PlantSafety.Domain.Observability.AutomatedIncidentDetection.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Observability.Commands;

/// <summary>Handles <see cref="DismissDetectionCommand"/>.</summary>
public sealed class DismissDetectionCommandHandler : IRequestHandler<DismissDetectionCommand, Result<AutomatedIncidentDetectionActionResult>>
{
    private readonly IAutomatedIncidentDetectionRepository _repository;
    private readonly ILogger<DismissDetectionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DismissDetectionCommandHandler(IAutomatedIncidentDetectionRepository repository, ILogger<DismissDetectionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AutomatedIncidentDetectionActionResult>> Handle(DismissDetectionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AutomatedIncidentDetectionId.FromGuid(request.AutomatedIncidentDetectionId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<AutomatedIncidentDetectionActionResult>.Failure(new ApplicationError(
                    "Observability.AutomatedIncidentDetection.NotFound",
                    "The requested automated incident detection could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Dismiss(
                reason: request.Reason,
                dismissedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<AutomatedIncidentDetectionActionResult>.Success(new AutomatedIncidentDetectionActionResult(
                aggregate.AutomatedIncidentDetectionId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dismiss AutomatedIncidentDetection {AutomatedIncidentDetectionId}", request.AutomatedIncidentDetectionId);

            return Result<AutomatedIncidentDetectionActionResult>.Failure(new ApplicationError(
                "Observability.AutomatedIncidentDetection.DismissFailed",
                "Unable to dismiss the detection at this time.",
                Guid.NewGuid()));
        }
    }
}
