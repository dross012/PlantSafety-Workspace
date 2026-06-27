// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertConstants.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical domain constants for Alerts. Defines validation limits,
//      text constraints, and invariant values used across the domain.
//      Complies with Canonical Constant Rule (1.9).
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Constants;

public static class AlertConstants
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int NotesMaxLength = 5000;

    public const int MetadataKeyMaxLength = 100;
    public const int MetadataValueMaxLength = 2000;

    public const int AssignmentUserMaxLength = 200;

    public const int SourceModuleMaxLength = 100;

    public const string MetadataKeyPattern = "^[A-Za-z0-9_.-]+$";
}
