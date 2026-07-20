// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Archival
//  Layer:          Application
//  File:           ArchivalJobDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Projection of a single row from [PlantSafety.ArchiveDB].Archival.ArchivalJob
//                  -- the audit trail written by every purge stored procedure run
//                  (Phase7-Governance/74-retention-archival.sql). Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Archival.Models;

/// <summary>Projection of a single Archival.ArchivalJob row (purge run history/status).</summary>
public sealed record ArchivalJobDto(
    Guid ArchivalJobId,
    Guid DatasetId,
    Guid TenantId,
    string PartitionKey,
    string SourceTier,
    string TargetTier,
    string Status,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    string? FailureReason,
    Guid? CorrelationId,
    DateTime CreatedUtc,
    Guid CreatedBy);
