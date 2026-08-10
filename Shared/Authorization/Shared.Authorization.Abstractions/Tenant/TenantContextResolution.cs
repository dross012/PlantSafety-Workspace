// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Tenant
//  File:           TenantContextResolution.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. The outcome of resolving a raw
//                  "X-Api-Key" header value into a TenantContext --
//                  TenantContextFactory's return type, mirroring
//                  CallerContextResolution exactly. Outcome matches
//                  PlantSafetyMetrics.TenantResolutionsPerformed's own
//                  documented tag values.
// ============================================================================
namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>The outcome category of a single tenant resolution attempt.</summary>
public enum TenantContextResolutionOutcome
{
    /// <summary>No "X-Api-Key" header was present on the request.</summary>
    NoKeyProvided,

    /// <summary>The presented key matched a registered tenant binding.</summary>
    Recognized,

    /// <summary>The presented key did not match any registered tenant binding.</summary>
    Unrecognized,
}

/// <summary>The resolved TenantContext and the outcome category that produced it.</summary>
public sealed record TenantContextResolution(TenantContext Context, TenantContextResolutionOutcome Outcome);
