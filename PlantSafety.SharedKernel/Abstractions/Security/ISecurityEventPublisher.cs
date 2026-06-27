/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               ISecurityEventPublisher.cs
    DESCRIPTION:        Abstraction for publishing security-related events (login, logout, access denied, permission
                        changes, policy evaluations) into the eventing/observability pipeline.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Security;

/// <summary>
/// Publishes security-related events for observability and audit.
/// MUST be SIEM-safe and deterministic.
/// </summary>
public interface ISecurityEventPublisher
{
    void PublishSecurityEvent(string eventName, string description);
}
