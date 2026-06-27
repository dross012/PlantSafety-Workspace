// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           EmergencyManagementNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.Exceptions;

/// <summary>Thrown when a <see cref="EmergencyManagement"/> aggregate lookup finds no match.</summary>
public sealed class EmergencyManagementNotFoundException : EmergencyManagementDomainException
{
    /// <summary>Creates a not-found exception for the given EmergencyManagement identity.</summary>
    public EmergencyManagementNotFoundException(EmergencyManagementId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "EmergencyManagement with id '{id}' was not found.",
            "EmergencyManagement.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
