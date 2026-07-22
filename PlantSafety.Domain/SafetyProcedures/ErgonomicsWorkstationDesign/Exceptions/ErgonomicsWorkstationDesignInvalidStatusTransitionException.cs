// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           ErgonomicsWorkstationDesignInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ErgonomicsWorkstationDesign lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Exceptions;

/// <summary>Thrown when a <see cref="ErgonomicsWorkstationDesign"/> lifecycle transition is not permitted.</summary>
public sealed class ErgonomicsWorkstationDesignInvalidStatusTransitionException : ErgonomicsWorkstationDesignDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public ErgonomicsWorkstationDesignInvalidStatusTransitionException(
        ErgonomicsWorkstationDesignId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'ErgonomicsWorkstationDesign' {id} from '{from}' to '{to}'.",
            "ErgonomicsWorkstationDesign.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}