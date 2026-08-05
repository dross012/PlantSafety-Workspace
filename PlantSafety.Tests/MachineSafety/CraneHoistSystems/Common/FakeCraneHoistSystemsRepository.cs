// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Common
//  Layer:          Tests (Unit)
//  File:           FakeCraneHoistSystemsRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    In-memory fake implementing both the Domain write repository and the
//                  Application read-model repository for CraneHoistSystems (mirroring the
//                  dual-interface shape of CraneHoistSystemsDapperRepository), so scenario/
//                  end-to-end/resilience/chaos/stress/recovery/failover tests can exercise full
//                  command+query flows through the real MediatR handlers without a database.
//                  Optional fault injection via FailNextCall for resilience/chaos/recovery tests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Specifications;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.ValueObjects.Ids;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;
using Agg = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems;
using IWriteRepo = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Interfaces.ICraneHoistSystemsRepository;
using IReadRepo = PlantSafety.Application.MachineSafety.CraneHoistSystems.Repositories.ICraneHoistSystemsRepository;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Common;

/// <summary>In-memory fake CraneHoistSystems repository implementing both the write and read-model contracts.</summary>
public sealed class FakeCraneHoistSystemsRepository : IWriteRepo, IReadRepo
{
    private readonly Dictionary<Guid, Agg> _store = new();

    /// <summary>When set, the next repository call throws this exception once, then clears itself.</summary>
    public Exception? FailNextCall { get; set; }

    private void MaybeThrow()
    {
        if (FailNextCall is null) return;
        var ex = FailNextCall;
        FailNextCall = null;
        throw ex;
    }

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public Task<Agg?> GetByIdAsync(CraneHoistSystemsId id, CancellationToken cancellationToken = default)
    {
        MaybeThrow();
        _store.TryGetValue(id.Value, out var agg);
        return Task.FromResult(agg);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public Task<IReadOnlyList<Agg>> ListAsync(CraneHoistSystemsSpecification specification, CancellationToken cancellationToken = default)
    {
        MaybeThrow();
        var predicate = specification.ToPredicate();
        IReadOnlyList<Agg> result = _store.Values.Where(predicate).ToList();
        return Task.FromResult(result);
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        MaybeThrow();
        _store[aggregate.CraneHoistSystemsId.Value] = aggregate;
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        MaybeThrow();
        _store[aggregate.CraneHoistSystemsId.Value] = aggregate;
        aggregate.ClearDomainEvents();
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="IReadRepo.GetCraneHoistSystemsItemsAsync"/>
    public Task<IReadOnlyList<CraneHoistSystemsItemDto>> GetCraneHoistSystemsItemsAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        MaybeThrow();
        IReadOnlyList<CraneHoistSystemsItemDto> items = _store.Values
            .Where(a => a.TenantId == tenantId
                        && (siteId is null || a.SiteId == siteId)
                        && a.CreatedAtUtc.Value >= from && a.CreatedAtUtc.Value <= to)
            .Select(ToDto)
            .ToList();
        return Task.FromResult(items);
    }

    /// <inheritdoc cref="IReadRepo.GetCraneHoistSystemsItemByIdAsync"/>
    public Task<CraneHoistSystemsItemDto?> GetCraneHoistSystemsItemByIdAsync(
        Guid tenantId, Guid craneHoistSystemsId, CancellationToken cancellationToken)
    {
        MaybeThrow();
        var agg = _store.Values.FirstOrDefault(a => a.TenantId == tenantId && a.CraneHoistSystemsId.Value == craneHoistSystemsId);
        return Task.FromResult(agg is null ? null : ToDto(agg));
    }

    /// <inheritdoc cref="IReadRepo.ListAssetsDueForInspectionAsync"/>
    public Task<IReadOnlyList<CraneHoistSystemsItemDto>> ListAssetsDueForInspectionAsync(
        Guid tenantId, Guid? siteId, DateTime asOfUtc, CancellationToken cancellationToken)
    {
        MaybeThrow();
        IReadOnlyList<CraneHoistSystemsItemDto> items = _store.Values
            .Where(a => a.TenantId == tenantId && (siteId is null || a.SiteId == siteId)
                        && (a.Status is PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums.CraneHoistSystemsStatus.InspectionDue
                            or PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums.CraneHoistSystemsStatus.InspectionOverdue
                            || (a.InspectionDueAtUtc is not null && a.InspectionDueAtUtc.Value.Value <= asOfUtc)))
            .Select(ToDto)
            .ToList();
        return Task.FromResult(items);
    }

    /// <inheritdoc cref="IReadRepo.ListExpiringOperatorQualificationsAsync"/>
    public Task<IReadOnlyList<CraneHoistSystemsItemDto>> ListExpiringOperatorQualificationsAsync(
        Guid tenantId, Guid? siteId, int withinDays, CancellationToken cancellationToken)
    {
        MaybeThrow();
        var now = DateTime.UtcNow;
        var horizon = now.AddDays(withinDays);
        IReadOnlyList<CraneHoistSystemsItemDto> items = _store.Values
            .Where(a => a.TenantId == tenantId && (siteId is null || a.SiteId == siteId)
                        && !a.OperatorQualificationSuspended
                        && a.OperatorQualificationExpiresAtUtc is not null
                        && a.OperatorQualificationExpiresAtUtc.Value.Value >= now
                        && a.OperatorQualificationExpiresAtUtc.Value.Value <= horizon)
            .Select(ToDto)
            .ToList();
        return Task.FromResult(items);
    }

    private static CraneHoistSystemsItemDto ToDto(Agg a) => new()
    {
        CraneHoistSystemsId = a.CraneHoistSystemsId.Value,
        TenantId = a.TenantId,
        SiteId = a.SiteId,
        AssetTag = a.AssetTag,
        AssetTitle = a.AssetTitle,
        AssetType = a.AssetType.ToString(),
        RatedLoadCapacityKg = a.RatedLoadCapacityKg,
        LastObservedLoadKg = a.LastObservedLoadKg,
        OverloadEventCount = a.OverloadEventCount,
        InspectionDueAtUtc = a.InspectionDueAtUtc?.Value,
        OperatorQualificationExpiresAtUtc = a.OperatorQualificationExpiresAtUtc?.Value,
        SystemStatus = a.Status.ToString(),
        CreatedAtUtc = a.CreatedAtUtc.Value,
        CreatedBy = a.CreatedBy.Value,
    };
}
