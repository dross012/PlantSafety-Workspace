// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Construction
//  File:           SafetyGraphMetadataExtractor.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Deterministic extraction of a
//                  SafetyGraphMetadata envelope from already-known tenant/
//                  correlation/timestamp values. Kept pure and free of any
//                  Shared.Authorization dependency -- mirrors Shared.
//                  Scheduling.Abstractions and Shared.PredictiveSafety.
//                  Abstractions, neither of which references Shared.
//                  Authorization directly; the real TenantContext/CallerContext
//                  composition happens only in Shared.SafetyGraph.Infrastructure's
//                  Tenant/Authentication layers.
// ============================================================================
using Shared.SafetyGraph.Abstractions.Graph;

namespace Shared.SafetyGraph.Abstractions.Construction;

/// <summary>Deterministically extracts a SafetyGraphMetadata envelope from already-known values.</summary>
public static class SafetyGraphMetadataExtractor
{
    /// <summary>Extracts a SafetyGraphMetadata carrying the current schema version, <paramref name="tenantId"/>, <paramref name="correlationId"/>, and <paramref name="issuedAtUtc"/>.</summary>
    public static SafetyGraphMetadata Extract(string tenantId, string correlationId, DateTime issuedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(correlationId);

        return new SafetyGraphMetadata(SafetyGraphMetadata.CurrentSchemaVersion, tenantId, correlationId, issuedAtUtc);
    }
}
