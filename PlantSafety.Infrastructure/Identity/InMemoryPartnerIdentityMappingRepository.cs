// AUTHOR: Enterprise Integration & Ecosystem Expansion Agent
// CREATED: 2026-08-02
// FILE DESCRIPTION: Default IPartnerIdentityMappingRepository implementation
//   (Phase 6 §2.2 "Tenant Federation" — Partner identity mapping).
// SchemaVersion: 1
//
// KNOWN LIMITATION (documented, not silently accepted): this is a process-local,
// thread-safe in-memory store, NOT backed by SQL Server/Dapper like every other
// repository in this codebase. This environment's stored-procedure/schema
// migration pipeline was out of scope for this pass (no SQL schema access was
// exercised here), and Azure\Identity.Application's own persistence (ARM/Entra ID
// managed-identity querying) is not a fit for application-user partner-identity
// federation data (see the Phase 6 §2.2 report notes on why PartnerIdentityMapping
// was NOT added to Azure\Identity.Application). The natural next step is a real
// Dapper-backed repository following the same
// Persistence\Dapper\Commands|Queries pattern used by every other aggregate in
// PlantSafety.Infrastructure — this class is the drop-in replacement seam
// (same interface, same DI registration point in InfrastructureServiceRegistration).
// Data registered here does NOT survive process restarts.

using System.Collections.Concurrent;
using PlantSafety.Domain.Identity.Aggregates;
using PlantSafety.Domain.Identity.Repositories;
using PlantSafety.Domain.Identity.ValueObjects;

namespace PlantSafety.Infrastructure.Identity;

/// <summary>
/// Thread-safe, process-local in-memory implementation of
/// <see cref="IPartnerIdentityMappingRepository"/>. See file-level remarks for the
/// documented limitation and the extension seam for a real SQL-backed implementation.
/// </summary>
public sealed class InMemoryPartnerIdentityMappingRepository : IPartnerIdentityMappingRepository
{
    private readonly ConcurrentDictionary<Guid, PartnerIdentityMapping> _byId = new();

    /// <inheritdoc />
    public Task<PartnerIdentityMapping?> GetByIdAsync(
        PartnerIdentityMappingId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var found = _byId.TryGetValue(id.Value, out var mapping) && mapping!.TenantId == tenantId
            ? mapping
            : null;

        return Task.FromResult(found);
    }

    /// <inheritdoc />
    public Task<PartnerIdentityMapping?> FindActiveAsync(
        Guid tenantId,
        string partnerIssuer,
        string partnerExternalId,
        CancellationToken cancellationToken = default)
    {
        var match = _byId.Values.FirstOrDefault(m =>
            m.TenantId == tenantId
            && m.IsActive
            && string.Equals(m.PartnerIssuer, partnerIssuer, StringComparison.OrdinalIgnoreCase)
            && string.Equals(m.PartnerExternalId, partnerExternalId, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(match);
    }

    /// <inheritdoc />
    public Task AddAsync(PartnerIdentityMapping aggregate, CancellationToken cancellationToken = default)
    {
        _byId[aggregate.PartnerIdentityMappingId.Value] = aggregate;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateAsync(PartnerIdentityMapping aggregate, CancellationToken cancellationToken = default)
    {
        _byId[aggregate.PartnerIdentityMappingId.Value] = aggregate;
        return Task.CompletedTask;
    }
}
