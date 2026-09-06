// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Contracts
//  File:           ContractMetadataFactory.cs
//  Created:        2026-08-10
//  Description:    Phase 12 deliverable. Pure composition of the already-
//                  resolved TenantContext (Phase 11, which itself already
//                  carries a CallerContext, Phase 9/10) and IClock (Phase
//                  2.1) into a ContractMetadata. No new state, no I/O --
//                  just field extraction from data that already exists by
//                  the time this is called. Lives in Infrastructure (not
//                  Abstractions) because Shared.Authorization.Abstractions
//                  is deliberately kept dependency-free (see its own
//                  csproj), and IClock is defined in Shared.Deterministic.
// ============================================================================
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;

namespace Shared.Authorization.Infrastructure.Contracts;

/// <summary>Composes an already-resolved TenantContext + IClock into a ContractMetadata.</summary>
public static class ContractMetadataFactory
{
    /// <summary>Creates a ContractMetadata from <paramref name="tenantContext"/> (which carries its own CallerContext) and <paramref name="clock"/>.</summary>
    public static ContractMetadata Create(TenantContext tenantContext, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);
        ArgumentNullException.ThrowIfNull(clock);

        return new ContractMetadata(
            CallerRole: tenantContext.Caller.Role.Name,
            CorrelationId: tenantContext.CorrelationId,
            MfaStatus: tenantContext.Caller.MfaStatus,
            TenantId: tenantContext.TenantId,
            TimestampUtc: clock.UtcNow);
    }
}
