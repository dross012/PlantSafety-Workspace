// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Audit
//  Layer:          SharedKernel
//  File:           SecretRotationAuditEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 6.6 (§6.3 Enterprise Secrets Management) domain event raised
//                  whenever a secret rotation is detected/notified. Rotation itself always
//                  happens Azure-side (Key Vault); this event is the platform's record that
//                  a rotation occurred, carrying ONLY the secret NAME, the rotation
//                  timestamp, and the triggering context (e.g. "EventGrid.WebhookNotification"
//                  or "Manual.OperatorTriggered") -- the secret VALUE is never carried by this
//                  event or any of its consumers. Lives in SharedKernel (not
//                  Infrastructure/Application) because it is a cross-cutting platform concern
//                  a future consumer in any layer/module might react to (e.g. cache
//                  invalidation, connection-pool recycling), not something scoped to a single
//                  bounded context. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Base;

namespace PlantSafety.SharedKernel.Audit;

/// <summary>
/// Raised whenever a secret rotation is detected/notified. Carries the secret NAME, the
/// rotation timestamp, and the triggering context only -- never the secret value.
/// </summary>
/// <param name="SecretName">The logical name of the secret that was rotated (never its value).</param>
/// <param name="RotatedAtUtc">UTC timestamp at which the rotation occurred (or was reported).</param>
/// <param name="TriggeringContext">
/// Human-readable description of what raised this event (e.g. "EventGrid.WebhookNotification",
/// "Manual.OperatorTriggered").
/// </param>
public sealed record SecretRotationAuditEvent(
    string SecretName,
    DateTime RotatedAtUtc,
    string TriggeringContext) : DomainEvent
{
    /// <inheritdoc/>
    public override string EventType => "Secrets.SecretRotated";
}
