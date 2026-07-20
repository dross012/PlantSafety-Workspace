// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Archival
//  Layer:          Application
//  File:           TierPolicyDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Projection of a single row from [PlantSafety.ArchiveDB].Archival.TierPolicy
//                  -- the retention window (in days) each purge stored procedure reads by
//                  Zone (Phase7-Governance/74-retention-archival.sql). Read-only in this
//                  module; there is no settings editor -- see the "Retention Settings"
//                  section of the UI for the documented scope decision. Additive-only
//                  evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Archival.Models;

/// <summary>Projection of a single Archival.TierPolicy row (retention window settings).</summary>
public sealed record TierPolicyDto(
    Guid TierPolicyId,
    string Zone,
    string SourceTier,
    string TargetTier,
    int TransitionAfterDays,
    DateTime CreatedUtc,
    Guid CreatedBy);
