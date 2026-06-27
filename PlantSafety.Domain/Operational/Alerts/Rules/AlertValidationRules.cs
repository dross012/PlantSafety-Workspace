// ============================================================================
//  File:           AlertValidationRules.cs
//  Created:        2026-05-26
//  Description:
//      Cross-field domain invariants for Alerts. Ensures title, description,
//      severity, type, and source module meet canonical requirements.
// ============================================================================


// ============================================================================
//  File:           AlertValidationRules.cs
//  Created:        2026-05-26
//  Description:
//      Cross-field domain invariants for Alerts. Ensures title, description,
//      severity, type, and source module meet canonical requirements.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Rules;

public static class AlertValidationRules
{
    public static bool IsValidTitle(string title)
        => !string.IsNullOrWhiteSpace(title) &&
           title.Length <= 200;

    public static bool IsValidDescription(string description)
        => !string.IsNullOrWhiteSpace(description) &&
           description.Length <= 2000;

    public static bool IsValidSourceModule(string source)
        => !string.IsNullOrWhiteSpace(source);

    public static bool IsValidSeverity(AlertSeverity severity)
        => severity.IsValid();

    public static bool IsValidType(AlertType type)
        => type.IsValid();
}
