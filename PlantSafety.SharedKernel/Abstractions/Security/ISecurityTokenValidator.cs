/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               ISecurityTokenValidator.cs
    DESCRIPTION:        Abstraction for validating security tokens (e.g., JWT, API keys) in a deterministic, SIEM-safe,
                        infrastructure-agnostic manner.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Security;

/// <summary>
/// Validates security tokens in an infrastructure-agnostic way.
/// MUST be deterministic and SIEM-safe.
/// </summary>
public interface ISecurityTokenValidator
{
    /// <summary>
    /// Returns true if the token is valid according to configured security policies.
    /// </summary>
    bool IsValid(string token);
}
