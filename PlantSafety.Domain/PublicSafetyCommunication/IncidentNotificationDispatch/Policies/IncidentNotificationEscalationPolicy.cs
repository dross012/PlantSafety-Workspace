// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IncidentNotificationEscalationPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy determining when a failed or unacknowledged IncidentNotification
//                  must escalate via RiskManagement, per the module's risk hook.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Policies;

/// <summary>Determines whether an <see cref="IncidentNotification"/> requires RiskManagement escalation.</summary>
public static class IncidentNotificationEscalationPolicy
{
    /// <summary>Maximum time a dispatched-but-unacknowledged notification may remain outstanding before escalation.</summary>
    public static readonly TimeSpan AcknowledgementSla = TimeSpan.FromMinutes(30);

    /// <summary>Returns <see langword="true"/> when the notification must escalate via RiskManagement.</summary>
    public static bool RequiresEscalation(PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification aggregate, UtcTimestamp nowUtc)
    {
        if (aggregate.Status == IncidentNotificationStatus.Failed)
            return true;

        if (aggregate.Status == IncidentNotificationStatus.Dispatched
            && aggregate.Channel.RequiresAcknowledgement
            && aggregate.DispatchedAtUtc is not null
            && nowUtc.Value - aggregate.DispatchedAtUtc.Value.Value > AcknowledgementSla)
            return true;

        return false;
    }
}
