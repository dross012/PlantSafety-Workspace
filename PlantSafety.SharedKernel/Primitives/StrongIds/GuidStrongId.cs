/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-29
    FILE:               GuidStrongId.cs
    DESCRIPTION:        Generic StrongId wrapper for scenarios where a domain-specific StrongId is not required. Used for
                        infrastructure, testing, and cross-module utilities.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Primitives.StrongIds;

/// <summary>
/// Generic StrongId wrapper for infrastructure and testing.
/// </summary>
public sealed record GuidStrongId : StrongId<GuidStrongId>
{
    public GuidStrongId() { }

    public GuidStrongId(Guid value) : base(value) { }
}
