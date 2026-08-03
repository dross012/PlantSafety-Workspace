// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models
//  Layer:          Abstractions
//  File:           HazardSummaryEntry.cs
//  Created:        2026-07-27
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Projection of a single hazard/risk record from
//                  [PlantSafety.CoreDB].[RiskMgmt].[Risk] (joined to its child
//                  [RiskMgmt].[ControlMeasure] rows). Column provenance -- both
//                  tables have real, tracked CREATE TABLE scripts (unlike
//                  dbo.AuditLog): PlantSafety.Infrastructure's RiskManagement\Sql\
//                  Tables\Risk.sql and \ControlMeasure.sql. Field mapping:
//                    - HazardId        <- Risk.RiskId
//                    - IdentifiedAtUtc <- Risk.CreatedAtUtc
//                    - Category        <- Risk.HazardSource (free-text hazard
//                                         source description -- mechanical,
//                                         electrical, chemical, ergonomic,
//                                         process, human factors; see
//                                         PlantSafety.Domain.RiskManagement.
//                                         Aggregates.Risk.HazardSource remarks)
//                    - RiskLevel       <- Risk.Category (the RiskLevel Smart Enum
//                                         code: Low/Medium/High/Critical -- see
//                                         PlantSafety.Domain.RiskManagement.Enums.
//                                         RiskLevel). NOTE: the underlying column
//                                         is literally named "Category" in
//                                         RiskMgmt.Risk, but it stores the
//                                         qualitative RiskLevel, not a hazard
//                                         category -- see Risk.cs's own XML doc:
//                                         "Qualitative risk level, derived from
//                                         the most recent RiskScore". Renamed
//                                         here to RiskLevel to avoid confusion
//                                         with this record's own Category field.
//                    - Description     <- Risk.RiskTitle
//                    - ControlMeasures <- RiskMgmt.ControlMeasure.Description,
//                                         one row per applied control measure,
//                                         for the matching RiskId, IsDeleted = 0,
//                                         in ImplementedAtUtc order.
//
//                  Shape matches Reporting.Domain.ValueObjects.HazardSummaryEntry
//                  field-for-field (Reporting/Reporting.Domain/ValueObjects/
//                  HazardSummaryData.cs) so Reporting.Infrastructure's future
//                  mapping is a direct 1:1 projection.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models;

/// <summary>
/// A single PlantSafety hazard/risk record ([RiskMgmt].[Risk]) with its associated
/// control measures ([RiskMgmt].[ControlMeasure]).
/// </summary>
/// <param name="HazardId">The stable identifier of the source Risk aggregate (RiskId).</param>
/// <param name="IdentifiedAtUtc">UTC timestamp the hazard was identified (Risk.CreatedAtUtc).</param>
/// <param name="Category">The hazard source/category (Risk.HazardSource).</param>
/// <param name="RiskLevel">The assessed qualitative risk level -- "Low", "Medium", "High", or "Critical" (Risk.Category).</param>
/// <param name="Description">Human-readable description of the hazard (Risk.RiskTitle).</param>
/// <param name="ControlMeasures">The control measures recorded against this hazard, by description, in Hierarchy-of-Controls/implementation order.</param>
public sealed record HazardSummaryEntry(
    string HazardId,
    DateTime IdentifiedAtUtc,
    string Category,
    string RiskLevel,
    string Description,
    IReadOnlyList<string> ControlMeasures);
