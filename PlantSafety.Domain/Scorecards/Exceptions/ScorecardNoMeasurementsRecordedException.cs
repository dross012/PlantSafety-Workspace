// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           ScorecardNoMeasurementsRecordedException.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when compilation is attempted for a scorecard with no recorded KPI
//                  measurements.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Scorecards.ValueObjects.Ids;

namespace PlantSafety.Domain.Scorecards.Exceptions;

/// <summary>Thrown when compilation is attempted for a scorecard with no recorded KPI measurements.</summary>
public sealed class ScorecardNoMeasurementsRecordedException : ScorecardDomainException
{
    /// <summary>Creates an exception for the given scorecard.</summary>
    public ScorecardNoMeasurementsRecordedException(
        ScorecardId id,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot compile Scorecard {id}: no KPI measurements have been recorded for this reporting period.",
            "Scorecard.NoMeasurementsRecorded",
            "Validation",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
