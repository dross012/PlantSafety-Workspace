// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.TestMatrix.Fakes
//  Layer:          Tests (Unit)
//  File:           InMemoryEvacuationModelingRepositories.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lightweight, deterministic in-memory fakes for the four DigitalTwin.
//                  EvacuationModeling domain write-repository interfaces (EvacuationRoutes,
//                  EvacuationZones, EgressSimulations, EvacuationBottlenecks). Used by the
//                  cross-cutting test-matrix suites (Functional/E2E/Scenario/Smoke/UAT/Resilience/
//                  Chaos/Failover/Recovery/Performance/Stress) to exercise real command handlers
//                  end-to-end through the real MediatR handler classes without a database.
//                  Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Specifications;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Specifications;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Specifications;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Specifications;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.ValueObjects.Ids;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix.Fakes;

/// <summary>Deterministic, thread-safe in-memory fake for <see cref="IEvacuationRouteRepository"/>.</summary>
public sealed class InMemoryEvacuationRouteRepository : IEvacuationRouteRepository
{
    private readonly Dictionary<Guid, EvacuationRoute> _store = new();

    /// <summary>Number of times AddAsync has been invoked (Stress-test call-count assertions).</summary>
    public int AddCallCount { get; private set; }

    /// <summary>Number of times UpdateAsync has been invoked.</summary>
    public int UpdateCallCount { get; private set; }

    /// <summary>When set, AddAsync throws this exception instead of persisting (Chaos-test fault injection).</summary>
    public Exception? FaultOnAdd { get; set; }

    /// <summary>When set, UpdateAsync throws this exception instead of persisting (Chaos-test fault injection).</summary>
    public Exception? FaultOnUpdate { get; set; }

    public Task<EvacuationRoute?> GetByIdAsync(EvacuationRouteId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.TryGetValue(id.Value, out var agg) ? agg : null);

    public Task<IReadOnlyList<EvacuationRoute>> ListAsync(EvacuationRouteSpecification specification, CancellationToken cancellationToken = default)
    {
        var predicate = specification.ToPredicate();
        IReadOnlyList<EvacuationRoute> results = _store.Values.Where(predicate).ToList();
        return Task.FromResult(results);
    }

    public Task AddAsync(EvacuationRoute aggregate, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        if (FaultOnAdd is not null) throw FaultOnAdd;

        _store[aggregate.EvacuationRouteId.Value] = aggregate;
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(EvacuationRoute aggregate, CancellationToken cancellationToken = default)
    {
        UpdateCallCount++;
        if (FaultOnUpdate is not null) throw FaultOnUpdate;

        _store[aggregate.EvacuationRouteId.Value] = aggregate;
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }
}

/// <summary>Deterministic, thread-safe in-memory fake for <see cref="IEvacuationZoneRepository"/>.</summary>
public sealed class InMemoryEvacuationZoneRepository : IEvacuationZoneRepository
{
    private readonly Dictionary<Guid, EvacuationZone> _store = new();

    public int AddCallCount { get; private set; }
    public int UpdateCallCount { get; private set; }
    public Exception? FaultOnAdd { get; set; }
    public Exception? FaultOnUpdate { get; set; }

    public Task<EvacuationZone?> GetByIdAsync(EvacuationZoneId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.TryGetValue(id.Value, out var agg) ? agg : null);

    public Task<IReadOnlyList<EvacuationZone>> ListAsync(EvacuationZoneSpecification specification, CancellationToken cancellationToken = default)
    {
        var predicate = specification.ToPredicate();
        IReadOnlyList<EvacuationZone> results = _store.Values.Where(predicate).ToList();
        return Task.FromResult(results);
    }

    public Task AddAsync(EvacuationZone aggregate, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        if (FaultOnAdd is not null) throw FaultOnAdd;

        _store[aggregate.EvacuationZoneId.Value] = aggregate;
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(EvacuationZone aggregate, CancellationToken cancellationToken = default)
    {
        UpdateCallCount++;
        if (FaultOnUpdate is not null) throw FaultOnUpdate;

        _store[aggregate.EvacuationZoneId.Value] = aggregate;
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }
}

/// <summary>Deterministic, thread-safe in-memory fake for <see cref="IEgressSimulationRunRepository"/>.</summary>
public sealed class InMemoryEgressSimulationRunRepository : IEgressSimulationRunRepository
{
    private readonly Dictionary<Guid, EgressSimulationRun> _store = new();

    public int AddCallCount { get; private set; }
    public int UpdateCallCount { get; private set; }
    public Exception? FaultOnAdd { get; set; }
    public Exception? FaultOnUpdate { get; set; }

    public Task<EgressSimulationRun?> GetByIdAsync(EgressSimulationRunId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.TryGetValue(id.Value, out var agg) ? agg : null);

    public Task<IReadOnlyList<EgressSimulationRun>> ListAsync(EgressSimulationRunSpecification specification, CancellationToken cancellationToken = default)
    {
        var predicate = specification.ToPredicate();
        IReadOnlyList<EgressSimulationRun> results = _store.Values.Where(predicate).ToList();
        return Task.FromResult(results);
    }

    public Task AddAsync(EgressSimulationRun aggregate, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        if (FaultOnAdd is not null) throw FaultOnAdd;

        _store[aggregate.EgressSimulationRunId.Value] = aggregate;
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(EgressSimulationRun aggregate, CancellationToken cancellationToken = default)
    {
        UpdateCallCount++;
        if (FaultOnUpdate is not null) throw FaultOnUpdate;

        _store[aggregate.EgressSimulationRunId.Value] = aggregate;
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }
}

/// <summary>Deterministic, thread-safe in-memory fake for <see cref="IEvacuationBottleneckRepository"/>.</summary>
public sealed class InMemoryEvacuationBottleneckRepository : IEvacuationBottleneckRepository
{
    private readonly Dictionary<Guid, EvacuationBottleneck> _store = new();

    public int AddCallCount { get; private set; }
    public int UpdateCallCount { get; private set; }
    public Exception? FaultOnAdd { get; set; }
    public Exception? FaultOnUpdate { get; set; }

    public Task<EvacuationBottleneck?> GetByIdAsync(EvacuationBottleneckId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.TryGetValue(id.Value, out var agg) ? agg : null);

    public Task<EvacuationBottleneck?> GetOpenForRouteAsync(EvacuationRouteId routeId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.Values.FirstOrDefault(b =>
            b.RouteId.Equals(routeId) &&
            b.Status != PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Enums.EvacuationBottleneckStatus.Mitigated));

    public Task<IReadOnlyList<EvacuationBottleneck>> ListAsync(EvacuationBottleneckSpecification specification, CancellationToken cancellationToken = default)
    {
        var predicate = specification.ToPredicate();
        IReadOnlyList<EvacuationBottleneck> results = _store.Values.Where(predicate).ToList();
        return Task.FromResult(results);
    }

    public Task AddAsync(EvacuationBottleneck aggregate, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        if (FaultOnAdd is not null) throw FaultOnAdd;

        _store[aggregate.EvacuationBottleneckId.Value] = aggregate;
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(EvacuationBottleneck aggregate, CancellationToken cancellationToken = default)
    {
        UpdateCallCount++;
        if (FaultOnUpdate is not null) throw FaultOnUpdate;

        _store[aggregate.EvacuationBottleneckId.Value] = aggregate;
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }
}
