// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Domain
//  File:           LoadSecurementCheckNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a LoadSecurementCheck aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Exceptions;

/// <summary>Thrown when a <see cref="LoadSecurementCheck"/> aggregate lookup finds no match.</summary>
public sealed class LoadSecurementCheckNotFoundException : LoadSecurementCheckDomainException
{
    /// <summary>Creates a not-found exception for the given LoadSecurementCheck identity.</summary>
    public LoadSecurementCheckNotFoundException(LoadSecurementCheckId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"LoadSecurementCheck with id '{id}' was not found.",
            "LoadSecurementCheck.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
