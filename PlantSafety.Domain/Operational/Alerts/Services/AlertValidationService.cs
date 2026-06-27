// ============================================================================
//  File:           AlertValidationService.cs
//  Created:        2026-05-26
//  Description:
//      Domain service for cross-field validation of Alerts. Ensures title,
//      description, severity, type, and source module meet canonical rules.
// ============================================================================

using PlantSafety.Domain.Operational.Alerts.Enums;
using PlantSafety.Domain.Operational.Alerts.Exceptions;
using PlantSafety.Domain.Operational.Alerts.Rules;
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Operational.Alerts.Services;

public sealed class AlertValidationService
{
    public static void ValidateForCreation(
        string title,
        string description,
        AlertType type,
        AlertSeverity severity,
        string sourceModule,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!AlertValidationRules.IsValidTitle(title))
            throw new InvalidAlertDomainException(
                "Invalid alert title.",
                "ALERT.INVALID_TITLE",
                "Validation",
                "Error",
                correlationId,
                causationId);

        if (!AlertValidationRules.IsValidDescription(description))
            throw new InvalidAlertDomainException(
                "Invalid alert description.",
                "ALERT.INVALID_DESCRIPTION",
                "Validation",
                "Error",
                correlationId,
                causationId);

        if (!AlertValidationRules.IsValidType(type))
            throw new InvalidAlertDomainException(
                "Invalid alert type.",
                "ALERT.INVALID_TYPE",
                "Validation",
                "Error",
                correlationId,
                causationId);

        if (!AlertValidationRules.IsValidSeverity(severity))
            throw new InvalidAlertDomainException(
                "Invalid alert severity.",
                "ALERT.INVALID_SEVERITY",
                "Validation",
                "Error",
                correlationId,
                causationId);

        if (!AlertValidationRules.IsValidSourceModule(sourceModule))
            throw new InvalidAlertDomainException(
                "Invalid source module.",
                "ALERT.INVALID_SOURCE_MODULE",
                "Validation",
                "Error",
                correlationId,
                causationId);
    }
}
