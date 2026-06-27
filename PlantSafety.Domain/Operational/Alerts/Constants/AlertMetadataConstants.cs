// ============================================================================
//  File:           AlertMetadataConstants.cs
//  Created:        2026-05-26
//  Description:
//      Constants for metadata key/value constraints.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Constants;

public static class AlertMetadataConstants
{
    public const int MaxKeyLength = 100;
    public const int MaxValueLength = 2000;

    public const string AllowedKeyPattern = "^[A-Za-z0-9_.-]+$";
}
