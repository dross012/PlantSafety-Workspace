// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           AuditFindingNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a AuditFinding aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Exceptions;

/// <summary>Thrown when a <see cref="AuditFinding"/> aggregate lookup finds no match.</summary>
public sealed class AuditFindingNotFoundException : AuditFindingDomainException
{
    /// <summary>Creates a not-found exception for the given AuditFinding identity.</summary>
    public AuditFindingNotFoundException(AuditFindingId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "AuditFinding with id '" + id + "' was not found.",
            "AuditFinding.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}