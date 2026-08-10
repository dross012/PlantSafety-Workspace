// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Identity
//  File:           CallerContextResolution.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. The outcome of resolving a raw
//                  "X-Api-Key" header value into a CallerContext --
//                  CallerContextFactory's return type. Outcome matches
//                  PlantSafetyMetrics.ApiKeyResolutionsPerformed's own
//                  documented tag values (NoKeyProvided/Recognized/
//                  Unrecognized).
// ============================================================================
using Shared.Authorization.Abstractions.Caller;

namespace Shared.Authorization.Abstractions.Identity;

/// <summary>The outcome category of a single API-key resolution attempt.</summary>
public enum CallerContextResolutionOutcome
{
    /// <summary>No "X-Api-Key" header was present on the request.</summary>
    NoKeyProvided,

    /// <summary>The presented key matched a registered key.</summary>
    Recognized,

    /// <summary>The presented key did not match any registered key.</summary>
    Unrecognized,
}

/// <summary>The resolved CallerContext and the outcome category that produced it.</summary>
public sealed record CallerContextResolution(CallerContext Caller, CallerContextResolutionOutcome Outcome);
