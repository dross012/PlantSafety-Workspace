// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Authorization policy names for the SafetyEvents module.
//   These string constants are registered with the ASP.NET Core authorization engine in the API project.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Policies;

/// <summary>Authorization policy name constants for SafetyEvents features.</summary>
public static class SafetyEventsAuthorizationPolicies
{
    /// <summary>Policy that grants access to create incident reports.</summary>
    public const string CanCreateIncident = "SafetyEvents.CanCreateIncident";

    /// <summary>Policy that grants access to update incident records.</summary>
    public const string CanUpdateIncident = "SafetyEvents.CanUpdateIncident";

    /// <summary>Policy that grants access to delete incident records.</summary>
    public const string CanDeleteIncident = "SafetyEvents.CanDeleteIncident";

    /// <summary>Policy that grants access to export incident data.</summary>
    public const string CanExportIncidents = "SafetyEvents.CanExportIncidents";

    /// <summary>Policy that grants access to create near miss reports.</summary>
    public const string CanCreateNearMiss = "SafetyEvents.CanCreateNearMiss";

    /// <summary>Policy that grants access to create observation records.</summary>
    public const string CanCreateObservation = "SafetyEvents.CanCreateObservation";

    /// <summary>Policy that grants access to create RCA records.</summary>
    public const string CanCreateRca = "SafetyEvents.CanCreateRca";

    /// <summary>
    /// Policy that grants access to declare a crisis event.
    /// Restricted to Safety Managers and above.
    /// </summary>
    public const string CanDeclareCrisis = "SafetyEvents.CanDeclareCrisis";

    /// <summary>Policy that grants access to activate emergency responses.</summary>
    public const string CanActivateEmergencyResponse = "SafetyEvents.CanActivateEmergencyResponse";

    /// <summary>Policy that grants access to schedule drills.</summary>
    public const string CanScheduleDrill = "SafetyEvents.CanScheduleDrill";
}
