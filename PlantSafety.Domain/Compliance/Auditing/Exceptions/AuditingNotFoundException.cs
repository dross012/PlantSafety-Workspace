// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Domain
//  File:           AuditingNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Auditing.Exceptions;

/// <summary>Thrown when a <see cref="Auditing"/> aggregate lookup finds no match.</summary>
public sealed class AuditingNotFoundException : AuditingDomainException
{
    /// <summary>Creates a not-found exception for the given Auditing identity.</summary>
    public AuditingNotFoundException(AuditingId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Auditing with id '{id}' was not found.",
            "Auditing.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
