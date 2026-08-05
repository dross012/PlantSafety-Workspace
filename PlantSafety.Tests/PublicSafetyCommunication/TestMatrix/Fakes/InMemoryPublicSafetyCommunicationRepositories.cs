// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.TestMatrix.Fakes
//  Layer:          Tests (Unit)
//  File:           InMemoryPublicSafetyCommunicationRepositories.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lightweight, deterministic in-memory fakes for the four PublicSafetyCommunication
//                  domain repository interfaces. Used by the cross-cutting test-matrix suites
//                  (Functional/E2E/Scenario/Smoke/UAT/Resilience/Chaos/Failover/Recovery/
//                  Performance/Stress) to exercise real command handlers end-to-end through the
//                  real MediatR handler classes without a database, mirroring the simple
//                  ROWVERSION-as-string ETag semantics of the real Dapper repositories (see
//                  usp_*_Update.sql: a stale/mismatched ETag yields zero rows affected, which the
//                  real repositories translate into ConcurrencyConflictException). Isolated,
//                  deterministic, no I/O.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Interfaces;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Specifications;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.ValueObjects.Ids;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Interfaces;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Specifications;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.ValueObjects.Ids;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Interfaces;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Specifications;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.ValueObjects.Ids;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Interfaces;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Specifications;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.ValueObjects.Ids;
using IncidentNotificationAggregate = PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification;
using PublicAlertBroadcastAggregate = PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Tests.PublicSafetyCommunication.TestMatrix.Fakes;

/// <summary>
/// Reflection-based deep-clone helper shared by every in-memory fake in this file. Aggregates in
/// this codebase have only private setters and a "for rehydration only" private constructor (see
/// <see cref="AggregateReflectionHydrator"/>), and the fakes must not hand out live references to
/// their internally stored instances -- if they did, a caller mutating the aggregate returned from
/// GetByIdAsync (as every command handler does, e.g. <c>aggregate.Acknowledge(...)</c>) would
/// silently corrupt the fake's store even when the subsequent UpdateAsync call fails (e.g. a stale
/// ETag), which would produce false negatives in the Failover/Recovery suite. Cloning on both the
/// way in and the way out gives the fakes real Dapper-repository round-trip fidelity.
/// </summary>
internal static class AggregateCloner
{
    private const System.Reflection.BindingFlags InstanceFlags =
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

    /// <summary>Produces an independent, deep-enough copy of <paramref name="source"/> for isolated in-memory storage.</summary>
    public static T Clone<T>(T source) where T : class
    {
        var clone = AggregateReflectionHydrator.CreateUninitialized<T>();

        foreach (var property in typeof(T).GetProperties(InstanceFlags))
        {
            if (property.GetSetMethod(nonPublic: true) is null || property.GetIndexParameters().Length > 0)
                continue;

            property.SetValue(clone, property.GetValue(source));
        }

        foreach (var field in typeof(T).GetFields(InstanceFlags))
        {
            if (!field.FieldType.IsGenericType || field.FieldType.GetGenericTypeDefinition() != typeof(List<>))
                continue;

            var sourceList = field.GetValue(source);
            var newList = Activator.CreateInstance(field.FieldType)!;
            if (sourceList is System.Collections.IEnumerable enumerable)
            {
                var addMethod = field.FieldType.GetMethod("Add")!;
                foreach (var item in enumerable)
                    addMethod.Invoke(newList, [item]);
            }

            field.SetValue(clone, newList);
        }

        return clone;
    }
}

/// <summary>Deterministic, thread-safe in-memory fake for <see cref="IPublicSafetyAgencyRepository"/>.</summary>
public sealed class InMemoryPublicSafetyAgencyRepository : IPublicSafetyAgencyRepository
{
    private readonly Dictionary<Guid, PublicSafetyAgency> _store = new();
    private readonly Dictionary<Guid, string> _etags = new();

    /// <summary>Number of times AddAsync has been invoked (Stress-test call-count assertions).</summary>
    public int AddCallCount { get; private set; }

    /// <summary>When set, AddAsync throws this exception instead of persisting (Chaos-test fault injection).</summary>
    public Exception? FaultOnAdd { get; set; }

    public Task<PublicSafetyAgency?> GetByIdAsync(PublicSafetyAgencyId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.TryGetValue(id.Value, out var agg) ? AggregateCloner.Clone(agg) : null);

    public Task<IReadOnlyList<PublicSafetyAgency>> ListAsync(PublicSafetyAgencySpecification specification, CancellationToken cancellationToken = default)
    {
        var predicate = specification.ToPredicate();
        IReadOnlyList<PublicSafetyAgency> results = _store.Values.Where(predicate).Select(AggregateCloner.Clone).ToList();
        return Task.FromResult(results);
    }

    public Task AddAsync(PublicSafetyAgency aggregate, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        if (FaultOnAdd is not null) throw FaultOnAdd;

        _store[aggregate.PublicSafetyAgencyId.Value] = AggregateCloner.Clone(aggregate);
        _etags[aggregate.PublicSafetyAgencyId.Value] = Guid.NewGuid().ToString("N");
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(PublicSafetyAgency aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        if (eTag is not null && _etags.TryGetValue(aggregate.PublicSafetyAgencyId.Value, out var current) && current != eTag)
            throw new ConcurrencyConflictException("PublicSafetyAgency ETag is stale.", Guid.Empty);

        _store[aggregate.PublicSafetyAgencyId.Value] = AggregateCloner.Clone(aggregate);
        _etags[aggregate.PublicSafetyAgencyId.Value] = Guid.NewGuid().ToString("N");
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }

    /// <summary>Returns the current fake ETag for an aggregate, or null if unknown.</summary>
    public string? GetETag(Guid id) => _etags.GetValueOrDefault(id);
}

/// <summary>Deterministic, thread-safe in-memory fake for <see cref="IIncidentNotificationRepository"/>.</summary>
public sealed class InMemoryIncidentNotificationRepository : IIncidentNotificationRepository
{
    private readonly Dictionary<Guid, IncidentNotificationAggregate> _store = new();
    private readonly Dictionary<Guid, string> _etags = new();

    public int AddCallCount { get; private set; }
    public Exception? FaultOnAdd { get; set; }
    public Exception? FaultOnUpdate { get; set; }

    public Task<IncidentNotificationAggregate?> GetByIdAsync(IncidentNotificationId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.TryGetValue(id.Value, out var agg) ? AggregateCloner.Clone(agg) : null);

    public Task<IReadOnlyList<IncidentNotificationAggregate>> ListAsync(IncidentNotificationSpecification specification, CancellationToken cancellationToken = default)
    {
        var predicate = specification.ToPredicate();
        IReadOnlyList<IncidentNotificationAggregate> results = _store.Values.Where(predicate).Select(AggregateCloner.Clone).ToList();
        return Task.FromResult(results);
    }

    public Task AddAsync(IncidentNotificationAggregate aggregate, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        if (FaultOnAdd is not null) throw FaultOnAdd;

        _store[aggregate.IncidentNotificationId.Value] = AggregateCloner.Clone(aggregate);
        _etags[aggregate.IncidentNotificationId.Value] = Guid.NewGuid().ToString("N");
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(IncidentNotificationAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        if (FaultOnUpdate is not null) throw FaultOnUpdate;
        if (eTag is not null && _etags.TryGetValue(aggregate.IncidentNotificationId.Value, out var current) && current != eTag)
            throw new ConcurrencyConflictException("IncidentNotification ETag is stale.", Guid.Empty);

        _store[aggregate.IncidentNotificationId.Value] = AggregateCloner.Clone(aggregate);
        _etags[aggregate.IncidentNotificationId.Value] = Guid.NewGuid().ToString("N");
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }

    public string? GetETag(Guid id) => _etags.GetValueOrDefault(id);
}

/// <summary>Deterministic, thread-safe in-memory fake for <see cref="IPublicAlertBroadcastRepository"/>.</summary>
public sealed class InMemoryPublicAlertBroadcastRepository : IPublicAlertBroadcastRepository
{
    private readonly Dictionary<Guid, PublicAlertBroadcastAggregate> _store = new();
    private readonly Dictionary<Guid, string> _etags = new();

    public int AddCallCount { get; private set; }
    public Exception? FaultOnAdd { get; set; }

    public Task<PublicAlertBroadcastAggregate?> GetByIdAsync(PublicAlertBroadcastId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.TryGetValue(id.Value, out var agg) ? AggregateCloner.Clone(agg) : null);

    public Task<IReadOnlyList<PublicAlertBroadcastAggregate>> ListAsync(PublicAlertBroadcastSpecification specification, CancellationToken cancellationToken = default)
    {
        var predicate = specification.ToPredicate();
        IReadOnlyList<PublicAlertBroadcastAggregate> results = _store.Values.Where(predicate).Select(AggregateCloner.Clone).ToList();
        return Task.FromResult(results);
    }

    public Task AddAsync(PublicAlertBroadcastAggregate aggregate, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        if (FaultOnAdd is not null) throw FaultOnAdd;

        _store[aggregate.PublicAlertBroadcastId.Value] = AggregateCloner.Clone(aggregate);
        _etags[aggregate.PublicAlertBroadcastId.Value] = Guid.NewGuid().ToString("N");
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(PublicAlertBroadcastAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        if (eTag is not null && _etags.TryGetValue(aggregate.PublicAlertBroadcastId.Value, out var current) && current != eTag)
            throw new ConcurrencyConflictException("PublicAlertBroadcast ETag is stale.", Guid.Empty);

        _store[aggregate.PublicAlertBroadcastId.Value] = AggregateCloner.Clone(aggregate);
        _etags[aggregate.PublicAlertBroadcastId.Value] = Guid.NewGuid().ToString("N");
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }
}

/// <summary>Deterministic, thread-safe, append-only in-memory fake for <see cref="ICommunicationLogEntryRepository"/>.</summary>
public sealed class InMemoryCommunicationLogEntryRepository : ICommunicationLogEntryRepository
{
    private readonly Dictionary<Guid, CommunicationLogEntry> _store = new();

    public int AddCallCount { get; private set; }
    public Exception? FaultOnAdd { get; set; }

    public Task<CommunicationLogEntry?> GetByIdAsync(CommunicationLogEntryId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.TryGetValue(id.Value, out var agg) ? AggregateCloner.Clone(agg) : null);

    public Task<IReadOnlyList<CommunicationLogEntry>> ListAsync(CommunicationLogEntrySpecification specification, CancellationToken cancellationToken = default)
    {
        var predicate = specification.ToPredicate();
        IReadOnlyList<CommunicationLogEntry> results = _store.Values.Where(predicate).Select(AggregateCloner.Clone).ToList();
        return Task.FromResult(results);
    }

    public Task AddAsync(CommunicationLogEntry aggregate, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        if (FaultOnAdd is not null) throw FaultOnAdd;

        _store[aggregate.CommunicationLogEntryId.Value] = AggregateCloner.Clone(aggregate);
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }
}
