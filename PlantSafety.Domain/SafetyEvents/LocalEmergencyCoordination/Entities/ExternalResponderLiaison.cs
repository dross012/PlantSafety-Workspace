// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           ExternalResponderLiaison.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Child entity representing a person assigned as the live-incident liaison to a
//                  specific external local emergency responder agency. Owns the LiaisonLogEntry
//                  communication trail for that liaison assignment.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Entities;

/// <summary>A person assigned to liaise with a specific external local emergency responder agency.</summary>
public sealed class ExternalResponderLiaison
{
    /// <summary>Unique identity of this liaison assignment.</summary>
    public ExternalResponderLiaisonId ExternalResponderLiaisonId { get; private set; }

    /// <summary>Identity of the parent LocalEmergencyCoordination aggregate.</summary>
    public LocalEmergencyCoordinationId LocalEmergencyCoordinationId { get; private set; }

    /// <summary>Identity of the SafetyEvents incident this liaison supports, if any.</summary>
    public Guid? IncidentId { get; private set; }

    /// <summary>Name of the external agency the liaison is assigned to.</summary>
    public string AgencyName { get; private set; } = string.Empty;

    /// <summary>Identity of the internal user assigned as liaison.</summary>
    public UserId LiaisonUserId { get; private set; }

    /// <summary>UTC instant the liaison was assigned.</summary>
    public UtcTimestamp AssignedAtUtc { get; private set; }

    /// <summary>Identity of the user who made the assignment.</summary>
    public UserId AssignedBy { get; private set; }

    private readonly List<LiaisonLogEntry> _logEntries = [];

    /// <summary>Communication log entries recorded against this liaison assignment.</summary>
    public IReadOnlyCollection<LiaisonLogEntry> LogEntries => new ReadOnlyCollection<LiaisonLogEntry>(_logEntries);

    /// <summary>For ORM rehydration only.</summary>
    private ExternalResponderLiaison() { }

    /// <summary>Creates a new ExternalResponderLiaison assignment.</summary>
    internal static ExternalResponderLiaison Assign(
        LocalEmergencyCoordinationId localEmergencyCoordinationId,
        Guid? incidentId,
        string agencyName,
        UserId liaisonUserId,
        UtcTimestamp assignedAtUtc,
        UserId assignedBy)
        => new()
        {
            ExternalResponderLiaisonId   = ExternalResponderLiaisonId.New(),
            LocalEmergencyCoordinationId = localEmergencyCoordinationId,
            IncidentId                   = incidentId,
            AgencyName                   = (agencyName ?? string.Empty).Trim(),
            LiaisonUserId                = liaisonUserId,
            AssignedAtUtc                = assignedAtUtc,
            AssignedBy                   = assignedBy,
        };

    /// <summary>Records a liaison communication log entry against this assignment. Retained with the incident record for the full incident retention period.</summary>
    internal LiaisonLogEntry LogCommunication(string message, UtcTimestamp loggedAtUtc, UserId loggedBy)
    {
        var entry = LiaisonLogEntry.Create(ExternalResponderLiaisonId, message, loggedAtUtc, loggedBy);
        _logEntries.Add(entry);
        return entry;
    }

    /// <summary>For read-model/ORM rehydration of the log-entry collection.</summary>
    internal void RehydrateLogEntries(IEnumerable<LiaisonLogEntry> entries)
    {
        _logEntries.Clear();
        _logEntries.AddRange(entries);
    }
}
