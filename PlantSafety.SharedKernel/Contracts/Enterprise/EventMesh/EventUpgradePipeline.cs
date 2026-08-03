// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Contracts.Enterprise.EventMesh
//  Layer:          SharedKernel
//  File:           EventUpgradePipeline.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.3 Event Versioning, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Chains zero or more <see cref="IEventUpgrader"/> steps to bring an
//                  <see cref="EnterpriseEventEnvelope"/> up to the latest registered schema version
//                  for its EventType, so a v1 envelope emitted (or archived, for replay) long before
//                  a v2/v3 upgrader existed can still be consumed by handlers written against the
//                  latest shape. Logs (without throwing) when a received envelope uses a
//                  <see cref="DeprecatedEventVersionRegistry">deprecated</see> version, satisfying
//                  Handbook Part XVI's deprecation/replay-compatibility rules without breaking
//                  processing. Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Logging;

namespace PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

/// <summary>
/// Upgrades an <see cref="EnterpriseEventEnvelope"/> to the latest schema version registered for its
/// <see cref="EnterpriseEventEnvelope.EventType"/> by chaining the applicable <see cref="IEventUpgrader"/>
/// steps in order.
/// </summary>
public sealed class EventUpgradePipeline
{
    private readonly ILookup<string, IEventUpgrader> _upgradersByEventType;
    private readonly ILogger<EventUpgradePipeline> _logger;

    /// <summary>Initialises the pipeline with every registered <see cref="IEventUpgrader"/>.</summary>
    public EventUpgradePipeline(IEnumerable<IEventUpgrader> upgraders, ILogger<EventUpgradePipeline> logger)
    {
        _upgradersByEventType = upgraders.ToLookup(u => u.EventType, StringComparer.Ordinal);
        _logger = logger;
    }

    /// <summary>
    /// Returns <paramref name="envelope"/> upgraded to the highest schema version reachable via the
    /// registered upgrade chain for its <see cref="EnterpriseEventEnvelope.EventType"/>. If no
    /// upgrader applies (including when none are registered for that EventType at all), the
    /// envelope is returned unchanged. Logs a warning (does not throw) if the incoming version is
    /// marked deprecated via <see cref="DeprecatedEventVersionRegistry"/>.
    /// </summary>
    public EnterpriseEventEnvelope UpgradeToLatest(EnterpriseEventEnvelope envelope)
    {
        if (DeprecatedEventVersionRegistry.TryGetDeprecationReason(envelope.EventType, envelope.SchemaVersion, out var reason))
        {
            _logger.LogWarning(
                "EnterpriseEventMesh: received deprecated {EventType} SchemaVersion={SchemaVersion} (EventId={EventId}): {Reason}. Processing continues.",
                envelope.EventType, envelope.SchemaVersion, envelope.EventId, reason);
        }

        var current = envelope;
        var candidates = _upgradersByEventType[envelope.EventType].ToDictionary(u => u.FromSchemaVersion);

        // Bounded by the number of distinct versions registered for this EventType -- cannot loop
        // forever even with a misconfigured/cyclic registration, since FromSchemaVersion strictly
        // increases each iteration and candidates is finite.
        while (candidates.TryGetValue(current.SchemaVersion, out var upgrader))
        {
            var upgradedPayload = upgrader.Upgrade(current.Payload);
            current = current with { SchemaVersion = upgrader.ToSchemaVersion, Payload = upgradedPayload };
        }

        return current;
    }
}
