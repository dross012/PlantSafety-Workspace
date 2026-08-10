// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Contracts
//  File:           ContractMetadataFactory.cs
//  Created:        2026-08-10
//  Description:    Phase 12 deliverable. Deterministic composition of a
//                  ContractMetadata from an already-resolved TenantContext.
//                  Every module's *Tenant.cs (RoutingTenant.cs,
//                  SchedulingTenant.cs, ...) and ApiKeyAuthenticationMiddleware
//                  call this exact method; no module composes ContractMetadata
//                  itself.
// ============================================================================
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;

namespace Shared.Authorization.Infrastructure.Contracts;

/// <summary>Deterministically composes a ContractMetadata from an already-resolved TenantContext.</summary>
public static class ContractMetadataFactory
{
    /// <summary>Composes a ContractMetadata carrying <paramref name="tenantContext"/>'s TenantId/CorrelationId and the current schema version.</summary>
    public static ContractMetadata Create(TenantContext tenantContext, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);
        ArgumentNullException.ThrowIfNull(clock);

        return new ContractMetadata(ContractMetadata.CurrentSchemaVersion, tenantContext.TenantId, tenantContext.CorrelationId, clock.UtcNow);
    }
}
