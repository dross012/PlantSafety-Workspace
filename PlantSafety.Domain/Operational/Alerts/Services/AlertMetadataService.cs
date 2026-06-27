// ============================================================================
//  File:           AlertMetadataService.cs
//  Created:        2026-05-26
//  Description:
//      Domain service for metadata operations. Ensures key/value invariants
//      and normalization. No infrastructure logic.
// ============================================================================

using PlantSafety.Domain.Operational.Alerts.Aggregates;
using PlantSafety.Domain.Operational.Alerts.Exceptions;
using PlantSafety.Domain.Operational.Alerts.Rules;
using PlantSafety.Domain.Operational.Alerts.ValueObjects;
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Operational.Alerts.Services;

public sealed class AlertMetadataService
{
    public static void AddMetadata(
        Alert alert,
        AlertMetadataKey key,
        AlertMetadataValue value)
    {
        if (!AlertMetadataRules.IsValidKey(key.Value))
            throw new AlertMetadataException(
                $"Invalid metadata key '{key.Value}'.",
                new CorrelationId(Guid.Empty),
                new CausationId(Guid.Empty));

        var normalized = new AlertMetadataValue(
            AlertMetadataRules.NormalizeValue(value.Value));

        alert.AddMetadata(key, normalized);
    }
}
