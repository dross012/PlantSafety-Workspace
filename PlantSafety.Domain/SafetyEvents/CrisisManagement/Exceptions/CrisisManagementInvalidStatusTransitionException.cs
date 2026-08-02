// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Domain
//  File:           CrisisManagementInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CrisisManagement lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.CrisisManagement.Exceptions;

/// <summary>Thrown when a CrisisManagement lifecycle transition is not permitted.</summary>
public sealed class CrisisManagementInvalidStatusTransitionException : CrisisManagementDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public CrisisManagementInvalidStatusTransitionException(
        CrisisManagementId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition CrisisManagement '{id}' from '{from}' to '{to}'.",
            "CrisisManagement.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
