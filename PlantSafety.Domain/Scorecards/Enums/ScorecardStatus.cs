// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           ScorecardStatus.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21 -- replaced the generic CRUD lifecycle with the real scorecard
//                  compilation/distribution lifecycle documented in the Safety Scorecards Module
//                  ("Scorecard Data Sources" -> "Scorecard Structure & Visualization" ->
//                  "Reporting & Distribution" -> "Continuous Improvement Loop").
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the Scorecard aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scorecards.Enums;

/// <summary>Lifecycle states for the Scorecard aggregate root.</summary>
public enum ScorecardStatus
{
    /// <summary>Scorecard period has been opened; KPI data collection from source modules is in progress.</summary>
    Draft = 0,

    /// <summary>All required KPI measurements for the period have been recorded and computed.</summary>
    Compiled = 1,

    /// <summary>Scorecard has been published/distributed (dashboards, leadership review, OSHA inspection, posted areas).</summary>
    Published = 2,

    /// <summary>The reporting period has ended and the scorecard is closed to further metric updates.</summary>
    Closed = 3,

    /// <summary>Record has been archived for long-term retention.</summary>
    Archived = 4,
}
