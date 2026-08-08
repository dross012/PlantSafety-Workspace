// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.Industries
//  Layer:          Infrastructure
//  File:           TenantFacilityRegistry.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Process-local, thread-safe, in-memory ITenantFacilityRegistry
//                  implementation. Deliberately in-memory-only: there is no
//                  persisted Facility aggregate anywhere in PlantSafety for
//                  this to be backed by (see TenantFacilityRegistration.cs's
//                  header) -- registrations do not survive a process
//                  restart. Whoever wires AddEventBusOrchestration() into a
//                  real host is responsible for calling Register for every
//                  tenant it wants EventOrchestrationService to interpret
//                  events for.
// ============================================================================
using System.Collections.Concurrent;

using Shared.SafetyData.Abstractions.Models.Industries.Orchestration;

namespace Shared.SafetyData.Infrastructure.Industries;

/// <summary>Process-local, thread-safe, in-memory <see cref="ITenantFacilityRegistry"/> implementation.</summary>
public sealed class TenantFacilityRegistry : ITenantFacilityRegistry
{
    private readonly ConcurrentDictionary<string, TenantFacilityRegistration> _registrations = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public void Register(TenantFacilityRegistration registration) =>
        _registrations[registration.TenantId] = registration;

    /// <inheritdoc />
    public bool TryResolve(string tenantId, out TenantFacilityRegistration? registration) =>
        _registrations.TryGetValue(tenantId, out registration);
}
