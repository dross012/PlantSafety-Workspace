// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.RiskManagement
//  Layer:          Domain
//  File:           RiskManagementNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.RiskManagement.Exceptions;

/// <summary>Thrown when a <see cref="RiskManagement"/> aggregate lookup finds no match.</summary>
public sealed class RiskManagementNotFoundException : RiskManagementDomainException
{
    /// <summary>Creates a not-found exception for the given RiskManagement identity.</summary>
    public RiskManagementNotFoundException(RiskManagementId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "RiskManagement with id '{id}' was not found.",
            "RiskManagement.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
