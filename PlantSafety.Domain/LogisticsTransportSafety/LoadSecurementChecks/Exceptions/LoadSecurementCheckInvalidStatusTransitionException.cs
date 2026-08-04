// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Domain
//  File:           LoadSecurementCheckInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a LoadSecurementCheck lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Exceptions;

/// <summary>Thrown when a <see cref="LoadSecurementCheck"/> lifecycle transition is not permitted.</summary>
public sealed class LoadSecurementCheckInvalidStatusTransitionException : LoadSecurementCheckDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public LoadSecurementCheckInvalidStatusTransitionException(
        LoadSecurementCheckId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'LoadSecurementCheck' {id} from '{from}' to '{to}'.",
            "LoadSecurementCheck.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
