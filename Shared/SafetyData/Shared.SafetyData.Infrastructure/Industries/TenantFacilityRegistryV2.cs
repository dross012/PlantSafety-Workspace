// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.Industries
//  Layer:          Infrastructure
//  File:           TenantFacilityRegistryV2.cs
//  Created:        2026-08-08
//  Description:    Phase C.8 (Path C: Operational Modernization):
//                  process-local, thread-safe, in-memory
//                  ITenantFacilityRegistryV2 implementation. Mirrors
//                  TenantFacilityRegistry's in-memory-only convention --
//                  deliberately not durable (same rationale: no real
//                  facility persistence for this reference layer to bind to).
// ============================================================================
using System.Collections.Concurrent;

using Shared.SafetyData.Abstractions.Models.Industries.Orchestration;

namespace Shared.SafetyData.Infrastructure.Industries;

/// <summary>Process-local, thread-safe, in-memory <see cref="ITenantFacilityRegistryV2"/> implementation.</summary>
public sealed class TenantFacilityRegistryV2 : ITenantFacilityRegistryV2
{
    private readonly ConcurrentDictionary<string, TenantFacilityMap> _mapsByTenantId = new(StringComparer.Ordinal);
    private readonly object _lock = new();

    /// <inheritdoc />
    public void RegisterFacility(string tenantId, Guid facilityId, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("tenantId must not be null or whitespace.", nameof(tenantId));
        }
        if (facilityId == Guid.Empty)
        {
            throw new ArgumentException("facilityId must not be Guid.Empty.", nameof(facilityId));
        }

        lock (_lock)
        {
            TenantFacilityMap existing = _mapsByTenantId.GetValueOrDefault(tenantId) ?? new TenantFacilityMap(tenantId, [], null);
            List<Guid> facilityIds = existing.FacilityIds.Contains(facilityId) ? [.. existing.FacilityIds] : [.. existing.FacilityIds, facilityId];
            Guid? primary = isPrimary || existing.PrimaryFacilityId is null ? facilityId : existing.PrimaryFacilityId;

            _mapsByTenantId[tenantId] = existing with { FacilityIds = facilityIds, PrimaryFacilityId = primary };
        }
    }

    /// <inheritdoc />
    public bool UnregisterFacility(string tenantId, Guid facilityId)
    {
        lock (_lock)
        {
            if (!_mapsByTenantId.TryGetValue(tenantId, out TenantFacilityMap? existing) || !existing.FacilityIds.Contains(facilityId))
            {
                return false;
            }

            List<Guid> remaining = existing.FacilityIds.Where(id => id != facilityId).ToList();
            Guid? primary = existing.PrimaryFacilityId == facilityId ? remaining.FirstOrDefault() : existing.PrimaryFacilityId;
            if (primary == Guid.Empty)
            {
                primary = null;
            }

            _mapsByTenantId[tenantId] = existing with { FacilityIds = remaining, PrimaryFacilityId = primary };
            return true;
        }
    }

    /// <inheritdoc />
    public bool TryGetFacilities(string tenantId, out IReadOnlyList<Guid> facilityIds)
    {
        if (_mapsByTenantId.TryGetValue(tenantId, out TenantFacilityMap? map))
        {
            facilityIds = map.FacilityIds;
            return true;
        }

        facilityIds = [];
        return false;
    }

    /// <inheritdoc />
    public bool TryResolvePrimaryFacility(string tenantId, out Guid facilityId)
    {
        if (_mapsByTenantId.TryGetValue(tenantId, out TenantFacilityMap? map) && map.PrimaryFacilityId is not null)
        {
            facilityId = map.PrimaryFacilityId.Value;
            return true;
        }

        facilityId = Guid.Empty;
        return false;
    }

    /// <inheritdoc />
    public bool CanRouteToFacility(string tenantId, Guid facilityId) =>
        _mapsByTenantId.TryGetValue(tenantId, out TenantFacilityMap? map) && map.FacilityIds.Contains(facilityId);

    /// <inheritdoc />
    public IReadOnlyList<Guid> GetDashboardFacilityIds(string tenantId)
    {
        if (!_mapsByTenantId.TryGetValue(tenantId, out TenantFacilityMap? map))
        {
            return [];
        }

        IEnumerable<Guid> ordered = map.PrimaryFacilityId is { } primary
            ? new[] { primary }.Concat(map.FacilityIds.Where(id => id != primary))
            : map.FacilityIds;

        return ordered.ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<TenantFacilityMap> GetAll() => _mapsByTenantId.Values.ToArray();
}
