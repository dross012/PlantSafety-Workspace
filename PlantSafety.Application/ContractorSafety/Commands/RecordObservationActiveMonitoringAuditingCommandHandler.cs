// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordObservationActiveMonitoringAuditingCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Interfaces;
using PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContractorSafety.Commands;

/// <summary>Handles <see cref="RecordObservationActiveMonitoringAuditingCommand"/>.</summary>
public sealed class RecordObservationActiveMonitoringAuditingCommandHandler : IRequestHandler<RecordObservationActiveMonitoringAuditingCommand, Result<ActiveMonitoringAuditingActionResult>>
{
    private readonly IActiveMonitoringAuditingRepository _repository;
    private readonly ILogger<RecordObservationActiveMonitoringAuditingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordObservationActiveMonitoringAuditingCommandHandler(IActiveMonitoringAuditingRepository repository, ILogger<RecordObservationActiveMonitoringAuditingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ActiveMonitoringAuditingActionResult>> Handle(RecordObservationActiveMonitoringAuditingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ActiveMonitoringAuditingId.FromGuid(request.ActiveMonitoringAuditingId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<ActiveMonitoringAuditingActionResult>.Failure(new ApplicationError(
                    "ContractorSafety.ActiveMonitoringAuditing.NotFound",
                    "The requested ActiveMonitoringAuditing record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordObservation(
                request.UnsafeConditionCount,
                request.ComplianceScore,
                request.FindingNotes,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<ActiveMonitoringAuditingActionResult>.Success(new ActiveMonitoringAuditingActionResult(
                aggregate.ActiveMonitoringAuditingId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute RecordObservation for ActiveMonitoringAuditing {ActiveMonitoringAuditingId}", request.ActiveMonitoringAuditingId);

            return Result<ActiveMonitoringAuditingActionResult>.Failure(new ApplicationError(
                "ContractorSafety.ActiveMonitoringAuditing.RecordObservationFailed",
                "Unable to RecordObservation the ActiveMonitoringAuditing record at this time.",
                Guid.NewGuid()));
        }
    }
}