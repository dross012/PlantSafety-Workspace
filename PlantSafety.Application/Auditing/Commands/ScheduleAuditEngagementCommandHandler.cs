// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ScheduleAuditEngagementCommand. Pure orchestration: loads the AuditEngagement aggregate, invokes its Schedule() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditEngagement.Interfaces;
using PlantSafety.Domain.Auditing.AuditEngagement.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Handles <see cref="ScheduleAuditEngagementCommand"/>.</summary>
public sealed class ScheduleAuditEngagementCommandHandler : IRequestHandler<ScheduleAuditEngagementCommand, Result<ScheduleAuditEngagementResult>>
{
    private readonly IAuditEngagementRepository _repository;
    private readonly ILogger<ScheduleAuditEngagementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ScheduleAuditEngagementCommandHandler(IAuditEngagementRepository repository, ILogger<ScheduleAuditEngagementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ScheduleAuditEngagementResult>> Handle(ScheduleAuditEngagementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AuditEngagementId.FromGuid(request.AuditEngagementId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<ScheduleAuditEngagementResult>.Failure(new ApplicationError(
                    "Auditing.AuditEngagement.NotFound",
                    "The requested AuditEngagement record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);
            var leadAuditorId = UserId.FromGuid(request.LeadAuditorId);

            aggregate.Schedule(
                leadAuditorId: leadAuditorId,
                scheduledAtUtc: UtcTimestamp.From(request.ScheduledAtUtc),
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<ScheduleAuditEngagementResult>.Success(new ScheduleAuditEngagementResult(
                aggregate.AuditEngagementId.Value,
                aggregate.Status.ToString(),
                aggregate.ScheduledAtUtc!.Value.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule AuditEngagement {AuditEngagementId}", request.AuditEngagementId);

            return Result<ScheduleAuditEngagementResult>.Failure(new ApplicationError(
                "Auditing.AuditEngagement.ScheduleFailed",
                "Unable to schedule the AuditEngagement at this time.",
                request.CorrelationId));
        }
    }
}
