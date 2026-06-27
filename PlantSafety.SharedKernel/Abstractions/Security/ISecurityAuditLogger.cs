/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               ISecurityAuditLogger.cs
    DESCRIPTION:        Canonical abstraction for security audit logging. Used to record security-sensitive operations such
                        as permission changes, policy updates, and access control decisions.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Security;

/// <summary>
/// Records security audit entries in a deterministic, SIEM-safe manner.
/// </summary>
public interface ISecurityAuditLogger
{
    /// <summary>
    /// Writes a security audit entry with a canonical action name and description.
    /// </summary>
    void Audit(string action, string description);
}
