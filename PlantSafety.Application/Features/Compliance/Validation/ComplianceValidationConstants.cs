// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Validation constants specific to the Compliance module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Compliance.Validation;

/// <summary>Validation constraint constants for Compliance features.</summary>
public static class ComplianceValidationConstants
{
    /// <summary>Maximum permit title length.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>Maximum permit description length.</summary>
    public const int MaxDescriptionLength = 4_000;

    /// <summary>Maximum approval notes length.</summary>
    public const int MaxApprovalNotesLength = 1_000;

    /// <summary>Minimum permit validity window in hours.</summary>
    public const int MinPermitWindowHours = 1;

    /// <summary>Maximum permit validity window in days.</summary>
    public const int MaxPermitWindowDays = 30;

    /// <summary>Maximum MOC title length.</summary>
    public const int MaxMocTitleLength = 200;

    /// <summary>Maximum MOC description length.</summary>
    public const int MaxMocDescriptionLength = 4_000;
}
