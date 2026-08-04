// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveLoadLedgerInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CognitiveLoadLedger lifecycle transition or intake action is not
//                  permitted in its current status. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Exceptions;

/// <summary>Thrown when a CognitiveLoadLedger lifecycle transition is not permitted.</summary>
public sealed class CognitiveLoadLedgerInvalidStatusTransitionException : CognitiveLoadAnalyticsDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public CognitiveLoadLedgerInvalidStatusTransitionException(
        CognitiveLoadLedgerId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'CognitiveLoadLedger' {id} from '{from}' to '{to}'.",
            "CognitiveLoadAnalytics.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
