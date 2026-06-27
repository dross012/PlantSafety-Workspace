// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementInvalidStatusTransitionException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Exceptions;

/// <summary>Thrown when a <see cref="IncidentManagement"/> lifecycle transition is not permitted.</summary>
public sealed class IncidentManagementInvalidStatusTransitionException : IncidentManagementDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public IncidentManagementInvalidStatusTransitionException(
        IncidentManagementId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'IncidentManagement' {id} from '{from}' to '{to}'.",
            "IncidentManagement.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
