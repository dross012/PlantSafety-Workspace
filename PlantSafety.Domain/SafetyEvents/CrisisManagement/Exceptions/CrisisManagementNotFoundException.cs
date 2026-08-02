// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Domain
//  File:           CrisisManagementNotFoundException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CrisisManagement aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.CrisisManagement.Exceptions;

/// <summary>Thrown when a CrisisManagement aggregate lookup finds no match.</summary>
public sealed class CrisisManagementNotFoundException : CrisisManagementDomainException
{
    /// <summary>Creates a not-found exception for the given CrisisManagement identity.</summary>
    public CrisisManagementNotFoundException(CrisisManagementId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"CrisisManagement with id '{id}' was not found.",
            "CrisisManagement.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
