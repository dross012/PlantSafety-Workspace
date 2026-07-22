// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Interfaces;
using PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Handles <see cref="BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommand"/>.</summary>
public sealed class BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommandHandler : IRequestHandler<BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommand, Result<RoutineSafetyAuditsIncidentReportingActionResult>>
{
    private readonly IRoutineSafetyAuditsIncidentReportingRepository _repository;
    private readonly ILogger<BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommandHandler(IRoutineSafetyAuditsIncidentReportingRepository repository, ILogger<BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RoutineSafetyAuditsIncidentReportingActionResult>> Handle(BeginCorrectiveActionRoutineSafetyAuditsIncidentReportingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = RoutineSafetyAuditsIncidentReportingId.FromGuid(request.RoutineSafetyAuditsIncidentReportingId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<RoutineSafetyAuditsIncidentReportingActionResult>.Failure(new ApplicationError(
                    "SafetyProcedures.RoutineSafetyAuditsIncidentReporting.NotFound",
                    "The requested RoutineSafetyAuditsIncidentReporting record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.BeginCorrectiveAction(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<RoutineSafetyAuditsIncidentReportingActionResult>.Success(new RoutineSafetyAuditsIncidentReportingActionResult(
                aggregate.RoutineSafetyAuditsIncidentReportingId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute BeginCorrectiveAction for RoutineSafetyAuditsIncidentReporting {RoutineSafetyAuditsIncidentReportingId}", request.RoutineSafetyAuditsIncidentReportingId);

            return Result<RoutineSafetyAuditsIncidentReportingActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.RoutineSafetyAuditsIncidentReporting.BeginCorrectiveActionFailed",
                "Unable to BeginCorrectiveAction the RoutineSafetyAuditsIncidentReporting record at this time.",
                Guid.NewGuid()));
        }
    }
}