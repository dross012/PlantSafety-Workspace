// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.SchemaGuard
//  Layer:          Application
//  File:           SchemaDriftEvaluator.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Pure comparison logic: expected migration manifest (filesystem scan)
//                  vs. actually-applied rows in [System].[SchemaMigrations] (per database).
//                  No I/O of its own — callers supply both inputs. Additive-only evolution.
// ============================================================================
using System.Collections.Generic;
using System.Linq;

using PlantSafety.Application.Operations.SchemaGuard.Models;

namespace PlantSafety.Application.Operations.SchemaGuard.Services;

/// <summary>Compares the expected migration manifest against a database's applied-migration records.</summary>
public static class SchemaDriftEvaluator
{
    /// <summary>
    /// Evaluates drift for one database. A script counts as "applied" only if it appears
    /// as a <c>Success = 1</c> row; a failed or missing row both count as drift.
    /// </summary>
    /// <param name="expectedScriptNames">
    /// The FULL manifest (all scripts across every phase folder). Filtered internally to
    /// scripts whose file name contains <paramref name="databaseKey"/> (case-insensitive) —
    /// the same per-database naming convention already used throughout Migrations\ (e.g.
    /// "20-CoreDB-rowversion.sql", "61-archivedb-procedures.sql", both backfilled under
    /// their matching database in 70-schema-migrations-tracking.sql). Shared/global scripts
    /// that don't embed a database name (e.g. cross-database governance scripts) are
    /// intentionally excluded from per-database drift, since they can't be confidently
    /// attributed to a single database from the file name alone — a known limitation, not
    /// a bug: false negatives on those scripts are possible, false positives are not.
    /// </param>
    public static SchemaDatabaseHealthDto Evaluate(
        string databaseKey,
        IReadOnlyList<string> expectedScriptNames,
        IReadOnlyList<SchemaMigrationRecordDto>? appliedMigrations)
    {
        if (appliedMigrations is null)
        {
            return new SchemaDatabaseHealthDto(databaseKey, "Unknown", System.Array.Empty<string>(), 0);
        }

        var relevantScripts = expectedScriptNames
            .Where(script => script.Contains(databaseKey, System.StringComparison.OrdinalIgnoreCase))
            .ToList();

        var appliedSuccessfulNames = appliedMigrations
            .Where(m => m.Success)
            .Select(m => m.ScriptName)
            .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

        var missing = relevantScripts
            .Where(script => !appliedSuccessfulNames.Contains(script))
            .OrderBy(script => script, System.StringComparer.OrdinalIgnoreCase)
            .ToList();

        var status = missing.Count == 0 ? "Current" : "Drifted";

        return new SchemaDatabaseHealthDto(databaseKey, status, missing, appliedSuccessfulNames.Count);
    }
}
