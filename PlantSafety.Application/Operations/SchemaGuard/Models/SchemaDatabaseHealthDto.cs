// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.SchemaGuard
//  Layer:          Application
//  File:           SchemaDatabaseHealthDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Schema-drift status for a single PlantSafety.* database: the migration
//                  scripts the Migrations\ folder tree says SHOULD have been applied
//                  (SchemaManifestScanner) versus what [System].[SchemaMigrations] says
//                  actually WAS applied (ISchemaVersionRepository). Additive-only evolution.
// ============================================================================
using System.Collections.Generic;

namespace PlantSafety.Application.Operations.SchemaGuard.Models;

/// <summary>Schema-drift status for a single database.</summary>
/// <param name="DatabaseKey">Logical database key (e.g. "CoreDB", "ArchiveDB") — matches SchemaGuardOptions.DatabaseConnectionStrings keys.</param>
/// <param name="Status">"Current" (no missing scripts), "Drifted" (one or more expected scripts not recorded as applied), or "Unknown" (could not connect / read the table).</param>
/// <param name="MissingScripts">Script names present in the manifest but not recorded as a successful row in [System].[SchemaMigrations].</param>
/// <param name="AppliedCount">Number of successful migration rows found for this database.</param>
public sealed record SchemaDatabaseHealthDto(
    string DatabaseKey,
    string Status,
    IReadOnlyList<string> MissingScripts,
    int AppliedCount);
