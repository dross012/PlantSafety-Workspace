// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Domain
//  File:           CultureNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Culture.Exceptions;

/// <summary>Thrown when a <see cref="Culture"/> aggregate lookup finds no match.</summary>
public sealed class CultureNotFoundException : CultureDomainException
{
    /// <summary>Creates a not-found exception for the given Culture identity.</summary>
    public CultureNotFoundException(CultureId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Culture with id '{id}' was not found.",
            "Culture.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
