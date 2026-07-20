// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Client-side DTOs for the Operations.SchemaGuard module. Mirror
//   PlantSafety.Application.Operations.SchemaGuard.Models.* returned by
//   PlantSafety.Api.Operations.SchemaGuard.Controllers.V1.OperationsSchemaGuardV1Controller.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.SchemaGuard.Models;

/// <summary>Schema-drift status for a single database.</summary>
public sealed class SchemaDatabaseHealthDto
{
    /// <summary>Gets the logical database key (e.g. "CoreDB").</summary>
    public string DatabaseKey { get; init; } = string.Empty;

    /// <summary>Gets "Current" | "Drifted" | "Unknown".</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the script names missing from this database's [System].[SchemaMigrations].</summary>
    public IReadOnlyList<string> MissingScripts { get; init; } = Array.Empty<string>();

    /// <summary>Gets the number of successful migration rows found for this database.</summary>
    public int AppliedCount { get; init; }
}

/// <summary>
/// Manual deployment instructions for a single missing migration script. Read-only --
/// this is never executed automatically, see the SchemaGuard dashboard's warning banner.
/// </summary>
public sealed class MigrationInstructionDto
{
    /// <summary>Gets the missing script's file name.</summary>
    public string ScriptName { get; init; } = string.Empty;

    /// <summary>Gets the absolute path to the script on disk, or null if not found.</summary>
    public string? ResolvedPath { get; init; }

    /// <summary>Gets the command an operator runs manually to apply the script.</summary>
    public string Instructions { get; init; } = string.Empty;
}
