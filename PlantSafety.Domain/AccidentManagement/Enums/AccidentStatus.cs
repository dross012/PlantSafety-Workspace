// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentStatus.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21 -- replaced the generic CRUD lifecycle with the real investigation
//                  lifecycle documented in the Accident Management section of
//                  PlantSafety_Program_Structure_extract.txt: "Securing the site, triaging injuries...
//                  Investigation & Root Cause Analysis... Corrective Actions (CAPA)...
//                  Regulatory Compliance... Documenting events and maintaining audit trails".
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the Accident aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AccidentManagement.Enums;

/// <summary>Lifecycle states for the Accident aggregate root.</summary>
public enum AccidentStatus
{
    /// <summary>Accident has been reported; site securing and injury triage may still be in progress.</summary>
    Reported = 0,

    /// <summary>An investigator has been assigned and the root-cause investigation is underway.</summary>
    UnderInvestigation = 1,

    /// <summary>Root Cause Analysis has been completed and a root cause recorded.</summary>
    RootCauseIdentified = 2,

    /// <summary>One or more Corrective and Preventive Actions (CAPA) have been created and are being tracked.</summary>
    CorrectiveActionsInProgress = 3,

    /// <summary>Required regulatory documentation (e.g. OSHA 300/300A/301) has been filed.</summary>
    RegulatoryFilingComplete = 4,

    /// <summary>Investigation is closed; all corrective actions verified effective (or waived) and documentation archived.</summary>
    Closed = 5,

    /// <summary>Record has been archived for long-term retention.</summary>
    Archived = 6,
}
