/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IAuthorizationService.cs
    DESCRIPTION:        Canonical authorization abstraction. Evaluates whether the current user or actor is allowed to
                        perform a given action under a specific permission requirement.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Security;

/// <summary>
/// Evaluates authorization decisions based on permission requirements.
/// MUST be deterministic and SIEM-safe.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Evaluates whether the current security context satisfies the given permission requirement.
    /// </summary>
    bool IsAuthorized(IPermissionRequirement requirement);
}
