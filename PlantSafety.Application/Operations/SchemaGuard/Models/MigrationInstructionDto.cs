// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.SchemaGuard
//  Layer:          Application
//  File:           MigrationInstructionDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    "Run Migration" surfaces instructions rather than auto-executing .sql
//                  files against a live database from the API process — same safety
//                  posture as Operations.Backup's restore-verify-only constraint. This DTO
//                  is what the UI's "Run Migration" affordance actually shows: the missing
//                  script's resolved path and the exact command an operator runs manually
//                  via the platform's normal deployment tooling (sqlcmd, matching how every
//                  other Migrations\*.sql script in this repo is applied). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operations.SchemaGuard.Models;

/// <summary>Deployment instructions for a single missing migration script.</summary>
/// <param name="ScriptName">The missing script's file name.</param>
/// <param name="ResolvedPath">Absolute path to the script on disk, or null if not found under Migrations\.</param>
/// <param name="Instructions">Human-readable command an operator runs manually to apply the script.</param>
public sealed record MigrationInstructionDto(
    string ScriptName,
    string? ResolvedPath,
    string Instructions);
