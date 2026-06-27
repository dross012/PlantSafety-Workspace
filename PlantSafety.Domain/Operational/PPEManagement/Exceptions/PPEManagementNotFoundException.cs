// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PPEManagement
//  Layer:          Domain
//  File:           PPEManagementNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PPEManagement.Exceptions;

/// <summary>Thrown when a <see cref="PPEManagement"/> aggregate lookup finds no match.</summary>
public sealed class PPEManagementNotFoundException : PPEManagementDomainException
{
    /// <summary>Creates a not-found exception for the given PPEManagement identity.</summary>
    public PPEManagementNotFoundException(PPEManagementId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "PPEManagement with id '{id}' was not found.",
            "PPEManagement.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
