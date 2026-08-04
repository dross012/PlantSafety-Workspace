// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveLoadLedgerNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CognitiveLoadLedger aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Exceptions;

/// <summary>Thrown when a CognitiveLoadLedger aggregate lookup finds no match.</summary>
public sealed class CognitiveLoadLedgerNotFoundException : CognitiveLoadAnalyticsDomainException
{
    /// <summary>Creates a not-found exception for the given CognitiveLoadLedger identity.</summary>
    public CognitiveLoadLedgerNotFoundException(CognitiveLoadLedgerId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"CognitiveLoadLedger with id '{id}' was not found.",
            "CognitiveLoadAnalytics.CognitiveLoadLedger.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
