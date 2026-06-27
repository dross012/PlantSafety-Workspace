// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Validation constants specific to the Operational module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Operational.Validation;

/// <summary>Validation constraint constants for Operational features.</summary>
public static class OperationalValidationConstants
{
    /// <summary>Maximum training course code length.</summary>
    public const int MaxCourseCodeLength = 50;

    /// <summary>Maximum training course name length.</summary>
    public const int MaxCourseNameLength = 200;

    /// <summary>Hazard likelihood scale minimum.</summary>
    public const int LikelihoodMin = 1;

    /// <summary>Hazard likelihood scale maximum.</summary>
    public const int LikelihoodMax = 5;

    /// <summary>Hazard consequence scale minimum.</summary>
    public const int ConsequenceMin = 1;

    /// <summary>Hazard consequence scale maximum.</summary>
    public const int ConsequenceMax = 5;

    /// <summary>Maximum training completion score.</summary>
    public const int MaxTrainingScore = 100;

    /// <summary>Minimum passing training score.</summary>
    public const int MinPassingScore = 0;
}
