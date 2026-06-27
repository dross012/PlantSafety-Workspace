// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Exceptions;

/// <summary>Thrown when a <see cref="IncidentManagement"/> aggregate lookup finds no match.</summary>
public sealed class IncidentManagementNotFoundException : IncidentManagementDomainException
{
    /// <summary>Creates a not-found exception for the given IncidentManagement identity.</summary>
    public IncidentManagementNotFoundException(IncidentManagementId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "IncidentManagement with id '{id}' was not found.",
            "IncidentManagement.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
