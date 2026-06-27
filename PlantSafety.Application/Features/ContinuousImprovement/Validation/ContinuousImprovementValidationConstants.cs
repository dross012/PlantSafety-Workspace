// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Validation constants specific to the ContinuousImprovement module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.ContinuousImprovement.Validation;

/// <summary>Validation constraint constants for ContinuousImprovement features.</summary>
public static class ContinuousImprovementValidationConstants
{
    /// <summary>Maximum CAPA title length.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>Maximum CAPA description length.</summary>
    public const int MaxDescriptionLength = 4_000;

    /// <summary>Maximum source event type discriminator length.</summary>
    public const int MaxSourceEventTypeLength = 100;

    /// <summary>Maximum notes length for CAPA closure.</summary>
    public const int MaxClosureNotesLength = 1_000;
}
