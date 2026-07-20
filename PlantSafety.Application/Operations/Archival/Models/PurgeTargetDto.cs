// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Archival
//  Layer:          Application
//  File:           PurgeTargetDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Static metadata describing one of the 5 live retention purge targets
//                  (Phase7-Governance/74-retention-archival.sql). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operations.Archival.Models;

/// <summary>Metadata describing a single live purge target (database, procedure, TierPolicy zone).</summary>
/// <param name="Target">The purge target selector.</param>
/// <param name="Zone">The Archival.TierPolicy.Zone key this target's retention window is read from.</param>
/// <param name="DatabaseName">The PlantSafety.* database the purge procedure lives in and runs against.</param>
/// <param name="ProcedureName">Fully schema-qualified stored procedure name.</param>
/// <param name="Description">Human-readable description, including any known caveats (e.g. the ErrorLog block).</param>
public sealed record PurgeTargetDto(
    PurgeTarget Target,
    string Zone,
    string DatabaseName,
    string ProcedureName,
    string Description);
