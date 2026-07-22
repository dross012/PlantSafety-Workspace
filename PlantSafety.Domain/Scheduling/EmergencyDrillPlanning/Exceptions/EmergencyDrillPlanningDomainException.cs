// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           EmergencyDrillPlanningDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the EmergencyDrillPlanning sub-module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the EmergencyDrillPlanning module.</summary>
public abstract class EmergencyDrillPlanningDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected EmergencyDrillPlanningDomainException(
        string message,
        string errorCode,
        string errorCategory,
        string severity,
        CorrelationId correlationId,
        CausationId causationId)
        : base(message, errorCode, errorCategory, severity, correlationId, causationId)
    {
    }
}