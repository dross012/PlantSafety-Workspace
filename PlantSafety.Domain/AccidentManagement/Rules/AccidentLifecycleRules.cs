// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentLifecycleRules.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21 -- realigned with the real investigation lifecycle
//                  (Reported -> UnderInvestigation -> RootCauseIdentified -> CorrectiveActionsInProgress
//                  -> RegulatoryFilingComplete -> Closed -> Archived).
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the Accident aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.AccidentManagement.Enums;

namespace PlantSafety.Domain.AccidentManagement.Rules;

/// <summary>Defines permitted lifecycle transitions for the Accident aggregate.</summary>
public static class AccidentLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when an investigator may be assigned from <paramref name="current"/>.</summary>
    public static bool CanAssignInvestigator(AccidentStatus current) => current == AccidentStatus.Reported;

    /// <summary>Returns <see langword="true"/> when a root cause may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordRootCause(AccidentStatus current) => current == AccidentStatus.UnderInvestigation;

    /// <summary>Returns <see langword="true"/> when a corrective action may be created from <paramref name="current"/>.</summary>
    public static bool CanCreateCorrectiveAction(AccidentStatus current)
        => current is AccidentStatus.RootCauseIdentified or AccidentStatus.CorrectiveActionsInProgress;

    /// <summary>Returns <see langword="true"/> when regulatory filing may be recorded from <paramref name="current"/>.</summary>
    public static bool CanFileRegulatoryReport(AccidentStatus current)
        => current is AccidentStatus.RootCauseIdentified or AccidentStatus.CorrectiveActionsInProgress;

    /// <summary>Returns <see langword="true"/> when the investigation may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(AccidentStatus current)
        => current is AccidentStatus.CorrectiveActionsInProgress or AccidentStatus.RegulatoryFilingComplete;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(AccidentStatus current) => current == AccidentStatus.Closed;
}
