// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Transporters
//  Layer:          Domain
//  File:           TransporterNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when a Transporter aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Transporters.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.Transporters.Exceptions;

/// <summary>Thrown when a Transporter aggregate lookup finds no match.</summary>
public sealed class TransporterNotFoundException : TransporterDomainException
{
    /// <summary>Creates a not-found exception for the given Transporter identity.</summary>
    public TransporterNotFoundException(TransporterId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Transporter with id '{id}' was not found.",
            "Transporter.NotFound", "NotFound", "Medium", correlationId, causationId)
    {
    }
}
