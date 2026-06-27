/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IAuthenticationService.cs
    DESCRIPTION:        Canonical authentication abstraction. Validates credentials or tokens and establishes the current
                        security context.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Security;

/// <summary>
/// Performs authentication and establishes the security context.
/// MUST NOT leak infrastructure details (JWT, cookies, etc.).
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Attempts to authenticate and returns a populated security context on success, or null on failure.
    /// </summary>
    ISecurityContext? Authenticate(string token);
}
