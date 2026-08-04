// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LiaisonLogEntry.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Immutable entity recording a single liaison communication with an external
//                  local emergency responder during a live incident. Retained with the incident
//                  record for the full incident retention period per data governance requirements.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Entities;

/// <summary>Records a single liaison communication entry against an <see cref="ExternalResponderLiaison"/>.</summary>
public sealed class LiaisonLogEntry
{
    /// <summary>Unique identity of this log entry.</summary>
    public LiaisonLogEntryId LiaisonLogEntryId { get; private set; }

    /// <summary>Identity of the parent ExternalResponderLiaison assignment.</summary>
    public ExternalResponderLiaisonId ExternalResponderLiaisonId { get; private set; }

    /// <summary>Communication content/summary.</summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>UTC instant the communication was logged.</summary>
    public UtcTimestamp LoggedAtUtc { get; private set; }

    /// <summary>Identity of the user who logged the communication.</summary>
    public UserId LoggedBy { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private LiaisonLogEntry() { }

    /// <summary>Creates a new liaison log entry.</summary>
    internal static LiaisonLogEntry Create(
        ExternalResponderLiaisonId externalResponderLiaisonId,
        string message,
        UtcTimestamp loggedAtUtc,
        UserId loggedBy)
        => new()
        {
            LiaisonLogEntryId          = LiaisonLogEntryId.New(),
            ExternalResponderLiaisonId = externalResponderLiaisonId,
            Message                    = (message ?? string.Empty).Trim(),
            LoggedAtUtc                = loggedAtUtc,
            LoggedBy                   = loggedBy,
        };
}
