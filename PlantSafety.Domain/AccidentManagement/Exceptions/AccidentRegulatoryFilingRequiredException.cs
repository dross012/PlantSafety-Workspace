// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentRegulatoryFilingRequiredException.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when investigation closure is attempted before required regulatory
//                  documentation has been filed ("Regulatory Compliance: Documenting events and
//                  maintaining audit trails for authorities" -- Accident Management).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.AccidentManagement.Exceptions;

/// <summary>Thrown when closure is attempted before required regulatory filing is complete.</summary>
public sealed class AccidentRegulatoryFilingRequiredException : AccidentDomainException
{
    /// <summary>Creates an exception for the given accident.</summary>
    public AccidentRegulatoryFilingRequiredException(
        AccidentId id,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot close Accident {id}: this accident is flagged as requiring regulatory filing, which has not yet been completed.",
            "Accident.RegulatoryFilingRequired",
            "Compliance",
            "High",
            correlationId,
            causationId)
    {
    }
}
