// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for AcknowledgeDetectionCommand.
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

/// <summary>Handles <see cref="AcknowledgeDetectionCommand"/>.</summary>
public sealed class AcknowledgeDetectionCommandHandler : IRequestHandler<AcknowledgeDetectionCommand, Result<AutomatedIncidentDetectionActionResult>>
{
    private readonly IAutomatedIncidentDetectionRepository _repository;
    private readonly ILogger<AcknowledgeDetectionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AcknowledgeDetectionCommandHandler(IAutomatedIncidentDetectionRepository repository, ILogger<AcknowledgeDetectionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AutomatedIncidentDetectionActionResult>> Handle(AcknowledgeDetectionCommand request, CancellationToken cancellationToken)
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

            var acknowledgedBy = UserId.FromGuid(request.AcknowledgedByUserId);

            aggregate.Acknowledge(
                acknowledgedBy: acknowledgedBy,
                acknowledgedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(acknowledgedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<AutomatedIncidentDetectionActionResult>.Success(new AutomatedIncidentDetectionActionResult(
                aggregate.AutomatedIncidentDetectionId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acknowledge AutomatedIncidentDetection {AutomatedIncidentDetectionId}", request.AutomatedIncidentDetectionId);

            return Result<AutomatedIncidentDetectionActionResult>.Failure(new ApplicationError(
                "Observability.AutomatedIncidentDetection.AcknowledgeFailed",
                "Unable to acknowledge the detection at this time.",
                Guid.NewGuid()));
        }
    }
}
