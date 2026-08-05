// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination
//  Layer:          Tests (Unit)
//  File:           LocalEmergencyCoordinationTestSupport.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Shared aggregate-construction helpers for the SafetyEvents.LocalEmergencyCoordination
//                  unit test matrix (Domain / Commands / Handlers / Validation / Functional / Scenario /
//                  Smoke / Acceptance / Compatibility / Interoperability / Resilience / Chaos / Security /
//                  Failover / Recovery / Performance / Stress). Kept local to this module's test folder
//                  per the module-scoped agent boundary; not added to the shared Tests.Common.Builders
//                  namespace. Isolated, deterministic, no I/O, no network.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Events;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;
using LocalEmergencyCoordinationAggregate = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;

/// <summary>Shared helpers for constructing LocalEmergencyCoordination aggregates and command envelopes in tests.</summary>
public static class LocalEmergencyCoordinationTestSupport
{
    /// <summary>
    /// Extracts the <see cref="LocalEmergencyCoordinationEventId"/> from any concrete
    /// LocalEmergencyCoordination domain event. <see cref="ILocalEmergencyCoordinationDomainEvent"/>
    /// is a marker interface only -- EventId lives on each concrete record -- so this centralizes the
    /// exhaustive switch in one place rather than repeating it (or reaching for <c>dynamic</c>)
    /// across every test file that needs to inspect a raised event's identity.
    /// </summary>
    public static Guid EventIdOf(ILocalEmergencyCoordinationDomainEvent evt) => evt switch
    {
        LocalEmergencyCoordinationCreatedEvent e => e.EventId.Value,
        MutualAidAgreementEstablishedEvent e => e.EventId.Value,
        JointResponsePlanActivatedEvent e => e.EventId.Value,
        JointResponsePlanDeactivatedEvent e => e.EventId.Value,
        ExternalResponderLiaisonAssignedEvent e => e.EventId.Value,
        LiaisonCommunicationLoggedEvent e => e.EventId.Value,
        JointDebriefRecordedEvent e => e.EventId.Value,
        LocalEmergencyCoordinationClosedEvent e => e.EventId.Value,
        LocalEmergencyCoordinationArchivedEvent e => e.EventId.Value,
        _ => throw new NotSupportedException($"Unrecognized LocalEmergencyCoordination domain event type: {evt.GetType().Name}"),
    };

    /// <summary>Creates a freshly-opened LocalEmergencyCoordination aggregate (status: Created).</summary>
    public static LocalEmergencyCoordinationAggregate NewCase(
        UserId? createdBy = null,
        UtcTimestamp? createdAtUtc = null,
        CorrelationId? correlationId = null,
        CausationId? causationId = null,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid? incidentId = null,
        string caseName = "Regional Wildfire Mutual Aid Case")
    {
        var by = createdBy ?? UserId.FromGuid(new Guid("00000002-0000-0000-0000-000000000002"));
        return LocalEmergencyCoordinationAggregate.Create(
            createdBy: by,
            createdAtUtc: createdAtUtc ?? UtcTimestamp.From(new DateTime(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc)),
            correlationId: correlationId ?? new CorrelationId(new Guid("00000003-0000-0000-0000-000000000003")),
            causationId: causationId ?? new CausationId(new Guid("00000004-0000-0000-0000-000000000004")),
            actor: PerformedByActor.User(by),
            tenantId: tenantId == default ? new Guid("00000001-0000-0000-0000-000000000001") : tenantId,
            siteId: siteId,
            incidentId: incidentId,
            caseName: caseName);
    }

    /// <summary>Opens a case and immediately establishes a mutual aid agreement, returning the case (Status: Active).</summary>
    public static LocalEmergencyCoordinationAggregate WithMutualAidAgreement(
        LocalEmergencyCoordinationAggregate? aggregate = null,
        string agencyName = "Riverside County Fire",
        string discipline = "Fire",
        int documentVersion = 1,
        UtcTimestamp? effectiveFromUtc = null,
        UtcTimestamp? renewalDueAtUtc = null)
    {
        var agg = aggregate ?? NewCase();
        agg.EstablishMutualAidAgreement(
            agencyName: agencyName,
            discipline: discipline,
            documentVersion: documentVersion,
            effectiveFromUtc: effectiveFromUtc ?? UtcTimestamp.From(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            renewalDueAtUtc: renewalDueAtUtc ?? UtcTimestamp.From(new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            establishedBy: UserId.FromGuid(new Guid("00000002-0000-0000-0000-000000000002")),
            establishedAtUtc: UtcTimestamp.From(new DateTime(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc)),
            actor: PerformedByActor.User(UserId.FromGuid(new Guid("00000002-0000-0000-0000-000000000002"))),
            correlationId: new CorrelationId(new Guid("00000003-0000-0000-0000-000000000003")),
            causationId: new CausationId(new Guid("00000004-0000-0000-0000-000000000004")));

        return agg;
    }

    /// <summary>Canonical test tenant identifier matching <see cref="PlantSafety.Tests.Common.TestBase.UnitTestBase.TestTenantId"/>.</summary>
    public static readonly Guid TenantId = new("00000001-0000-0000-0000-000000000001");

    /// <summary>Canonical test user identifier matching <see cref="PlantSafety.Tests.Common.TestBase.UnitTestBase.TestUserId"/>.</summary>
    public static readonly Guid UserId1 = new("00000002-0000-0000-0000-000000000002");
}

/// <summary>
/// Minimal in-memory fake of the Domain-layer write-side repository, for Functional/Scenario/
/// Acceptance-style tests that chain multiple command handlers together against the same
/// in-process "persisted" aggregate without a real database. Not registered anywhere outside this
/// module's own test folder.
/// </summary>
public sealed class InMemoryLocalEmergencyCoordinationRepository
    : PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces.ILocalEmergencyCoordinationRepository
{
    private readonly Dictionary<Guid, LocalEmergencyCoordinationAggregate> _store = new();

    /// <summary>Number of writes (Add + Update) performed against this fake, for call-count assertions.</summary>
    public int WriteCount { get; private set; }

    /// <inheritdoc/>
    public Task<LocalEmergencyCoordinationAggregate?> GetByIdAsync(LocalEmergencyCoordinationId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id.Value, out var agg);
        return Task.FromResult(agg is not null && agg.TenantId == tenantId ? agg : null);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<LocalEmergencyCoordinationAggregate>> ListAsync(
        PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Specifications.LocalEmergencyCoordinationSpecification specification,
        CancellationToken cancellationToken = default)
    {
        var predicate = specification.ToPredicate();
        IReadOnlyList<LocalEmergencyCoordinationAggregate> matches = _store.Values.Where(predicate).ToList();
        return Task.FromResult(matches);
    }

    /// <inheritdoc/>
    public Task AddAsync(LocalEmergencyCoordinationAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.LocalEmergencyCoordinationId.Value] = aggregate;
        WriteCount++;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UpdateAsync(LocalEmergencyCoordinationAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        _store[aggregate.LocalEmergencyCoordinationId.Value] = aggregate;
        WriteCount++;
        return Task.CompletedTask;
    }
}
