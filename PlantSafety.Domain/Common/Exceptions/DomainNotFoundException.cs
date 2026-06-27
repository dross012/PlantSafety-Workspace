// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           DomainNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Thrown when an aggregate or entity is requested by identity but cannot
//      be found in the persistence store. Maps to HTTP 404 at the API layer.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Exceptions;

/// <summary>Thrown when an aggregate or entity lookup by identity finds no match.</summary>
public class DomainNotFoundException : DomainException
{
    /// <summary>Creates a not-found exception for the given entity and identity.</summary>
    public DomainNotFoundException(
        string entityName,
        string id,
        CorrelationId correlationId,
        CausationId causationId)
        : base(
            $"{entityName} with id '{id}' was not found.",
            $"{entityName}.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
