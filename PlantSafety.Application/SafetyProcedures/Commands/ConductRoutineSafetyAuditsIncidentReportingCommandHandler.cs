// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ConductRoutineSafetyAuditsIncidentReportingCommand.
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

/// <summary>Handles <see cref="ConductRoutineSafetyAuditsIncidentReportingCommand"/>.</summary>
public sealed class ConductRoutineSafetyAuditsIncidentReportingCommandHandler : IRequestHandler<ConductRoutineSafetyAuditsIncidentReportingCommand, Result<RoutineSafetyAuditsIncidentReportingActionResult>>
{
    private readonly IRoutineSafetyAuditsIncidentReportingRepository _repository;
    private readonly ILogger<ConductRoutineSafetyAuditsIncidentReportingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ConductRoutineSafetyAuditsIncidentReportingCommandHandler(IRoutineSafetyAuditsIncidentReportingRepository repository, ILogger<ConductRoutineSafetyAuditsIncidentReportingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RoutineSafetyAuditsIncidentReportingActionResult>> Handle(ConductRoutineSafetyAuditsIncidentReportingCommand request, CancellationToken cancellationToken)
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

            aggregate.Conduct(
                request.FindingCount,
                request.ComplianceScore,
                UserId.FromGuid(request.ConductedByUserId),
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
            _logger.LogError(ex, "Failed to execute Conduct for RoutineSafetyAuditsIncidentReporting {RoutineSafetyAuditsIncidentReportingId}", request.RoutineSafetyAuditsIncidentReportingId);

            return Result<RoutineSafetyAuditsIncidentReportingActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.RoutineSafetyAuditsIncidentReporting.ConductFailed",
                "Unable to Conduct the RoutineSafetyAuditsIncidentReporting record at this time.",
                Guid.NewGuid()));
        }
    }
}