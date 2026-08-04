// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for BeginInvestigationTransportIncidentCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Interfaces;
using PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Handles <see cref="BeginInvestigationTransportIncidentCommand"/>.</summary>
public sealed class BeginInvestigationTransportIncidentCommandHandler : IRequestHandler<BeginInvestigationTransportIncidentCommand, Result<TransportIncidentActionResult>>
{
    private readonly ITransportIncidentRepository _repository;
    private readonly ILogger<BeginInvestigationTransportIncidentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BeginInvestigationTransportIncidentCommandHandler(ITransportIncidentRepository repository, ILogger<BeginInvestigationTransportIncidentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TransportIncidentActionResult>> Handle(BeginInvestigationTransportIncidentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = TransportIncidentId.FromGuid(request.TransportIncidentId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<TransportIncidentActionResult>.Failure(new ApplicationError(
                    "LogisticsTransportSafety.TransportIncidents.NotFound",
                    "The requested TransportIncident record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.BeginInvestigation(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<TransportIncidentActionResult>.Success(new TransportIncidentActionResult(
                aggregate.TransportIncidentId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute BeginInvestigation for TransportIncident {TransportIncidentId}", request.TransportIncidentId);

            return Result<TransportIncidentActionResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.TransportIncidents.BeginInvestigationFailed",
                "Unable to BeginInvestigation for the TransportIncident record at this time.",
                Guid.NewGuid()));
        }
    }
}
