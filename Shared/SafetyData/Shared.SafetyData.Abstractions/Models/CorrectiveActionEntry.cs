// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models
//  Layer:          Abstractions
//  File:           CorrectiveActionEntry.cs
//  Created:        2026-07-27
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Projection of a single corrective-action record from
//                  [PlantSafety.CoreDB].[ContinuousImprovement].[CAPA]. This is
//                  the CAPA table with a real, tracked CREATE TABLE script
//                  (PlantSafety.Infrastructure's ContinuousImprovement\Sql\
//                  Tables\CAPA.sql), backing ICAPARepository.GetActionsAsync via
//                  usp_CAPA_GetActions.
//
//                  JUDGMENT CALL (flagged): PlantSafety.Infrastructure also has a
//                  second, separate corrective-action concept --
//                  CorrectiveActionDapperRepository, calling
//                  [ContinuousImprovement].[usp_CorrectiveAction_Create]/
//                  [usp_CorrectiveAction_GetById]/[usp_CorrectiveAction_List]
//                  against an implied [ContinuousImprovement].[CorrectiveAction]
//                  table. That table/those procedures have NO tracked CREATE
//                  TABLE / CREATE PROCEDURE script anywhere in the repository
//                  (same "pre-existing, unversioned" situation this module's
//                  sibling Shared.AuditLogging already documented for
//                  dbo.AuditLog) -- only the C# read-model record shape
//                  (CorrectiveActionReadModel: CaId, TenantId, SourceEventId,
//                  SourceEventType, Title, Description, ActionType, Priority,
//                  AssignedToWorkerId, DueAtUtc, Status, CompletedAtUtc,
//                  LastModifiedAtUtc, ETag) is confirmed. This module maps to
//                  [ContinuousImprovement].[CAPA] instead, since it has a
//                  verified, version-controlled schema (Handbook rule: never
//                  invent column names) -- see this module's docs\README.md for
//                  the full rationale and a note for whoever later verifies the
//                  live schema and may want to re-target usp_GetCorrectiveActions
//                  at [ContinuousImprovement].[CorrectiveAction] instead/as well.
//
//                  Field mapping (CAPA.sql columns):
//                    - CorrectiveActionId <- CapaId
//                    - RaisedAtUtc        <- CreatedAtUtc
//                    - Category           <- ActionType
//                    - Status             <- Status
//                    - Description        <- Title (CAPA has no separate
//                                            free-text Description column --
//                                            Title is the only descriptive
//                                            field on the verified schema)
//                    - DueDateUtc         <- DueDateUtc
//
//                  Shape matches Reporting.Domain.ValueObjects.
//                  CorrectiveActionEntry field-for-field (Reporting/
//                  Reporting.Domain/ValueObjects/CorrectiveActionReportData.cs)
//                  so Reporting.Infrastructure's future mapping is a direct 1:1
//                  projection.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models;

/// <summary>A single PlantSafety corrective action record ([ContinuousImprovement].[CAPA]).</summary>
/// <param name="CorrectiveActionId">The stable identifier of the source CAPA record (CapaId).</param>
/// <param name="RaisedAtUtc">UTC timestamp the corrective action was raised (CAPA.CreatedAtUtc).</param>
/// <param name="Category">The corrective action category (CAPA.ActionType).</param>
/// <param name="Status">The current lifecycle status of the corrective action (CAPA.Status).</param>
/// <param name="Description">Human-readable description of the corrective action (CAPA.Title).</param>
/// <param name="DueDateUtc">The UTC due date for completion, if any (CAPA.DueDateUtc).</param>
public sealed record CorrectiveActionEntry(
    string CorrectiveActionId,
    DateTime RaisedAtUtc,
    string Category,
    string Status,
    string Description,
    DateTime? DueDateUtc);
