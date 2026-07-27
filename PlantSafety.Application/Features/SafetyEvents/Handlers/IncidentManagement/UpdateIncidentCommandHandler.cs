// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// UPDATED: 2026-07-27 -- Rewired through the Domain IncidentManagement aggregate: the current row
//   is loaded via IIncidentReadRepository (read-before-write), reconstructed with
//   IncidentManagement.Rehydrate, then mutated via UpdateDetails so the aggregate's invariants
//   (e.g. no updates once Archived -- IncidentManagementInvalidStatusTransitionException) are
//   enforced here rather than only implicitly by the [SafetyEvents].[Incident] IsDeleted=0 SQL
//   filter. A missing/soft-deleted incident now surfaces the domain-specific
//   IncidentManagementNotFoundException instead of the previous, less accurate
//   ConcurrencyConflictException (an incident row that no longer exists is not a concurrency
//   conflict). Persistence still uses IIncidentRepository.UpdateAsync's existing
//   ISNULL/COALESCE partial-update stored procedure -- the raw null-means-unchanged request
//   fields are still what is sent to SQL, NOT the aggregate's resolved/merged values, so only
//   fields the caller actually changed are written. See
//   docs/architecture/plantsafety-architecture.md Sec.5.1.
// FILE DESCRIPTION: MediatR handler for UpdateIncidentCommand.
//   By the time this handler executes: auth checked, inputs validated, transaction open, ETag checked by ConcurrencyBehavior.
// SchemaVersion: 2

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Caching;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Update;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Enums;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Exceptions;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using IncidentAggregate = PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>
/// Handles <see cref="UpdateIncidentCommand"/>.
/// Loads the current Incident, rehydrates the <see cref="IncidentAggregate"/>, applies
/// <c>UpdateDetails</c> (enforcing lifecycle invariants), persists only the changed fields via
/// the existing partial-update repository call, and stages an outbox event.
/// </summary>
public sealed class UpdateIncidentCommandHandler : ICommandHandler<UpdateIncidentCommand, Guid>
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IIncidentReadRepository _incidentReadRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UpdateIncidentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public UpdateIncidentCommandHandler(
        IIncidentRepository incidentRepository,
        IIncidentReadRepository incidentReadRepository,
        ICacheService cacheService,
        ILogger<UpdateIncidentCommandHandler> logger)
    {
        _incidentRepository = incidentRepository;
        _incidentReadRepository = incidentReadRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(UpdateIncidentCommand request, CancellationToken cancellationToken)
    {
        var correlationId = CorrelationId.FromGuid(request.CorrelationId);
        var causationId = CausationId.FromGuid(request.CausationId ?? request.CorrelationId);
        var incidentManagementId = IncidentManagementId.FromGuid(request.IncidentId);

        var currentRow = await _incidentReadRepository.GetByIdAsync(
            tenantId: request.TenantId,
            incidentId: request.IncidentId,
            cancellationToken: cancellationToken);

        if (currentRow is null)
        {
            throw new IncidentManagementNotFoundException(incidentManagementId, correlationId, causationId);
        }

        var aggregate = IncidentAggregate.Rehydrate(
            incidentManagementId: incidentManagementId,
            tenantId: currentRow.TenantId,
            siteId: currentRow.SiteId,
            status: Enum.Parse<IncidentManagementStatus>(currentRow.Status),
            title: currentRow.Title,
            description: currentRow.Description,
            severity: currentRow.Severity,
            location: currentRow.Location,
            incidentOccurredAtUtc: UtcTimestamp.From(currentRow.IncidentOccurredAtUtc.UtcDateTime),
            reportedAtUtc: UtcTimestamp.From(currentRow.ReportedAtUtc.UtcDateTime),
            reportedByWorkerId: UserId.FromGuid(currentRow.ReportedByWorkerId),
            assignedInvestigatorId: currentRow.AssignedInvestigatorId is Guid investigatorId
                ? UserId.FromGuid(investigatorId)
                : null,
            archivedAtUtc: null,
            deletionReason: null,
            createdAtUtc: UtcTimestamp.From(currentRow.CreatedAtUtc.UtcDateTime),
            createdBy: UserId.FromGuid(currentRow.CreatedBy));

        var actorId = UserId.FromGuid(request.ActorId);

        aggregate.UpdateDetails(
            title: request.Title,
            description: request.Description,
            severity: request.Severity,
            location: request.Location,
            assignedInvestigatorId: request.AssignedInvestigatorId is Guid assignedId
                ? UserId.FromGuid(assignedId)
                : null,
            nowUtc: UtcTimestamp.Now(),
            actor: PerformedByActor.User(actorId),
            correlationId: correlationId,
            causationId: causationId);

        await _incidentRepository.UpdateAsync(
            aggregate: aggregate,
            title: request.Title,
            description: request.Description,
            severity: request.Severity,
            location: request.Location,
            assignedInvestigatorId: request.AssignedInvestigatorId,
            eTag: request.ETag!,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        // GetIncidentByIdQuery.CacheKey uses this exact "incidents:{TenantId}:{IncidentId}" format
        // (ICacheableQuery) -- without this, a read immediately after this write can return a
        // stale cached IncidentDto until the entry's TTL expires (a real read-after-write
        // consistency gap, previously undetectable because IDistributedCache was unreachable --
        // see InitTestHost.ConfigureWebHost's in-memory-cache override remarks).
        await _cacheService.RemoveAsync($"incidents:{request.TenantId}:{request.IncidentId}", cancellationToken);

        _logger.LogInformation(
            "Incident {IncidentId} updated for Tenant {TenantId} by Actor {ActorId} CorrelationId {CorrelationId}",
            request.IncidentId, request.TenantId, request.ActorId, request.CorrelationId);

        return request.IncidentId;
    }
}
