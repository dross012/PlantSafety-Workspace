/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IPermissionRequirement.cs
    DESCRIPTION:        Canonical permission requirement abstraction. Represents a strong-typed, explicit permission check
                        used by the authorization pipeline.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Security;

/// <summary>
/// Represents a strong-typed permission requirement.
/// </summary>
public interface IPermissionRequirement
{
    /// <summary>
    /// Canonical permission name (e.g., "Routing.View", "Scheduling.Edit").
    /// MUST be normalized and deterministic.
    /// </summary>
    string PermissionName { get; }
}
