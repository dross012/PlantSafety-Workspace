// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for EscalateDetectionToIncidentCommand.
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

/// <summary>Handles <see cref="EscalateDetectionToIncidentCommand"/>.</summary>
public sealed class EscalateDetectionToIncidentCommandHandler : IRequestHandler<EscalateDetectionToIncidentCommand, Result<AutomatedIncidentDetectionActionResult>>
{
    private readonly IAutomatedIncidentDetectionRepository _repository;
    private readonly ILogger<EscalateDetectionToIncidentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EscalateDetectionToIncidentCommandHandler(IAutomatedIncidentDetectionRepository repository, ILogger<EscalateDetectionToIncidentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AutomatedIncidentDetectionActionResult>> Handle(EscalateDetectionToIncidentCommand request, CancellationToken cancellationToken)
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

            aggregate.EscalateToIncident(
                incidentReferenceId: request.IncidentReferenceId,
                escalatedAtUtc: UtcTimestamp.Now(),
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
            _logger.LogError(ex, "Failed to escalate AutomatedIncidentDetection {AutomatedIncidentDetectionId} to incident", request.AutomatedIncidentDetectionId);

            return Result<AutomatedIncidentDetectionActionResult>.Failure(new ApplicationError(
                "Observability.AutomatedIncidentDetection.EscalateToIncidentFailed",
                "Unable to escalate the detection to an incident at this time.",
                Guid.NewGuid()));
        }
    }
}
