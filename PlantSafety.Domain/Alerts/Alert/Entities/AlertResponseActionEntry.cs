// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertResponseActionEntry.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Entity recording a single response action taken against an Alert (e.g.
//                  "Dispatch first responders", "Lockout equipment"), per each category's
//                  "Response Instructions" and "14. Alert Logging & Reporting: ... Response
//                  actions".
//                  Additive-only evolution.
// ============================================================================
using AlertId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertId;

namespace PlantSafety.Domain.Alerts.Alert.Entities;

/// <summary>Records a single response action taken against an <see cref="Aggregates.Alert"/>.</summary>
public sealed class AlertResponseActionEntry
{
    /// <summary>Unique identity of this response-action record.</summary>
    public Guid ResponseActionId { get; private set; }

    /// <summary>Identity of the parent Alert aggregate.</summary>
    public AlertId AlertId { get; private set; }

    /// <summary>Short description of the action taken (e.g. "Evacuate", "Lockout equipment").</summary>
    public string ActionDescription { get; private set; }

    /// <summary>UTC instant the action was taken.</summary>
    public UtcTimestamp TakenAtUtc { get; private set; }

    /// <summary>Actor who performed the action.</summary>
    public PerformedByActor TakenBy { get; private set; }

    /// <summary>Optional free-text notes about the action.</summary>
    public string? Notes { get; private set; }

    /// <summary>For rehydration only.</summary>
    private AlertResponseActionEntry() { ActionDescription = string.Empty; }

    /// <summary>Creates a new response-action record.</summary>
    public AlertResponseActionEntry(
        Guid responseActionId,
        AlertId alertId,
        string actionDescription,
        UtcTimestamp takenAtUtc,
        PerformedByActor takenBy,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(actionDescription))
            throw new ArgumentException("ActionDescription must not be null or whitespace.", nameof(actionDescription));

        ResponseActionId  = responseActionId;
        AlertId           = alertId;
        ActionDescription = actionDescription.Trim();
        TakenAtUtc        = takenAtUtc;
        TakenBy           = takenBy;
        Notes             = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
