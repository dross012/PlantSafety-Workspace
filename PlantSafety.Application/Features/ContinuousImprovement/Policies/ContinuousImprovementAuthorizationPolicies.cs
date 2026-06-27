// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Authorization policy names for the ContinuousImprovement module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.ContinuousImprovement.Policies;

/// <summary>Authorization policy name constants for ContinuousImprovement features.</summary>
public static class ContinuousImprovementAuthorizationPolicies
{
    /// <summary>Policy that grants access to create CAPA records.</summary>
    public const string CanCreateCapa = "ContinuousImprovement.CanCreateCapa";

    /// <summary>Policy that grants access to update CAPA records.</summary>
    public const string CanUpdateCapa = "ContinuousImprovement.CanUpdateCapa";

    /// <summary>Policy that grants access to close CAPA records.</summary>
    public const string CanCloseCapa = "ContinuousImprovement.CanCloseCapa";

    /// <summary>Policy that grants access to create hazard assessments.</summary>
    public const string CanCreateHazardAssessment = "ContinuousImprovement.CanCreateHazardAssessment";

    /// <summary>Policy that grants access to initiate LOTO procedures.</summary>
    public const string CanInitiateLoto = "ContinuousImprovement.CanInitiateLoto";

    /// <summary>Policy that grants access to perform JSA.</summary>
    public const string CanPerformJsa = "ContinuousImprovement.CanPerformJsa";
}
