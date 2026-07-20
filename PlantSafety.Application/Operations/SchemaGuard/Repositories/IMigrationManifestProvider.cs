// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.SchemaGuard
//  Layer:          Application
//  File:           IMigrationManifestProvider.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Provides the manifest of migration script names that SHOULD have been
//                  applied, discovered by scanning PlantSafety.Infrastructure\Persistence\
//                  Migrations\**\*.sql once and caching the result (implemented in
//                  Infrastructure, since filesystem access is an infrastructure concern).
//                  Additive-only evolution.
// ============================================================================
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlantSafety.Application.Operations.SchemaGuard.Repositories;

/// <summary>Supplies the expected migration-script manifest, scanned from disk and cached.</summary>
public interface IMigrationManifestProvider
{
    /// <summary>Returns every migration script file name found under the Migrations folder tree (cached after first call).</summary>
    Task<IReadOnlyList<string>> GetExpectedScriptNamesAsync(CancellationToken cancellationToken);

    /// <summary>Resolves the absolute path of a script by file name, or null if not found in the manifest.</summary>
    Task<string?> ResolveScriptPathAsync(string scriptName, CancellationToken cancellationToken);
}
