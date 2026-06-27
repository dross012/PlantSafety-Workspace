// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Authorization policy names for the Operational module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Operational.Policies;

/// <summary>Authorization policy name constants for Operational features.</summary>
public static class OperationalAuthorizationPolicies
{
    /// <summary>Policy that grants access to manage worker records.</summary>
    public const string CanManageWorkers = "Operational.CanManageWorkers";

    /// <summary>Policy that grants access to assign training.</summary>
    public const string CanAssignTraining = "Operational.CanAssignTraining";

    /// <summary>Policy that grants access to complete training records.</summary>
    public const string CanCompleteTraining = "Operational.CanCompleteTraining";

    /// <summary>Policy that grants access to issue PPE.</summary>
    public const string CanIssuePPE = "Operational.CanIssuePPE";

    /// <summary>Policy that grants access to manage schedules.</summary>
    public const string CanManageSchedules = "Operational.CanManageSchedules";

    /// <summary>Policy that grants access to create hazard assessments.</summary>
    public const string CanCreateHazardAssessment = "Operational.CanCreateHazardAssessment";
}
