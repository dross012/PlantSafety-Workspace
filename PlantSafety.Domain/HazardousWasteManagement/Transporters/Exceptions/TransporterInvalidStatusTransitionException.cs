// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Transporters
//  Layer:          Domain
//  File:           TransporterInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when a Transporter lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Transporters.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.Transporters.Exceptions;

/// <summary>Thrown when a Transporter lifecycle transition is not permitted.</summary>
public sealed class TransporterInvalidStatusTransitionException : TransporterDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public TransporterInvalidStatusTransitionException(
        TransporterId id, string from, string to, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'Transporter' {id} from '{from}' to '{to}'.",
            "Transporter.InvalidTransition", "Lifecycle", "Medium", correlationId, causationId)
    {
    }
}
