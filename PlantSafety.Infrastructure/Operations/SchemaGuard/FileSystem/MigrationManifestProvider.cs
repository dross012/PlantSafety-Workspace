// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.SchemaGuard
//  Layer:          Infrastructure
//  File:           MigrationManifestProvider.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Scans PlantSafety.Infrastructure\Persistence\Migrations\**\*.sql once
//                  and caches the result in-process (the folder tree does not change at
//                  runtime, only at deploy time — a fresh process picks up new scripts).
//                  Additive-only evolution.
// ============================================================================
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlantSafety.Application.Operations.SchemaGuard.Repositories;
using PlantSafety.Infrastructure.Operations.SchemaGuard.Options;

namespace PlantSafety.Infrastructure.Operations.SchemaGuard.FileSystem;

/// <summary>Scans the Migrations folder tree for .sql script names, cached after the first call.</summary>
public sealed class MigrationManifestProvider : IMigrationManifestProvider
{
    private readonly SchemaGuardOptions _options;
    private readonly ILogger<MigrationManifestProvider> _logger;

    private readonly SemaphoreSlim _scanLock = new(1, 1);
    private Dictionary<string, string>? _scriptNameToPath;

    /// <summary>Initialises dependencies.</summary>
    public MigrationManifestProvider(IOptions<SchemaGuardOptions> options, ILogger<MigrationManifestProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GetExpectedScriptNamesAsync(CancellationToken cancellationToken)
    {
        var map = await EnsureScannedAsync(cancellationToken);
        return map.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <inheritdoc/>
    public async Task<string?> ResolveScriptPathAsync(string scriptName, CancellationToken cancellationToken)
    {
        var map = await EnsureScannedAsync(cancellationToken);
        return map.TryGetValue(scriptName, out var path) ? path : null;
    }

    private async Task<Dictionary<string, string>> EnsureScannedAsync(CancellationToken cancellationToken)
    {
        if (_scriptNameToPath is not null)
        {
            return _scriptNameToPath;
        }

        await _scanLock.WaitAsync(cancellationToken);
        try
        {
            if (_scriptNameToPath is not null)
            {
                return _scriptNameToPath;
            }

            if (!Directory.Exists(_options.MigrationsRootPath))
            {
                _logger.LogWarning("Schema manifest scan: Migrations root path {Path} does not exist.", _options.MigrationsRootPath);
                _scriptNameToPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                return _scriptNameToPath;
            }

            var files = Directory.GetFiles(_options.MigrationsRootPath, "*.sql", SearchOption.AllDirectories);

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                // Duplicate file names across phase folders should not happen; if they do,
                // keep the first one found and log it rather than silently overwriting.
                if (!map.TryAdd(name, file))
                {
                    _logger.LogWarning("Schema manifest scan: duplicate script name {ScriptName} found at {Path}; keeping first match {ExistingPath}.",
                        name, file, map[name]);
                }
            }

            _logger.LogInformation("Schema manifest scan complete: {Count} migration scripts discovered under {Path}.", map.Count, _options.MigrationsRootPath);

            _scriptNameToPath = map;
            return _scriptNameToPath;
        }
        finally
        {
            _scanLock.Release();
        }
    }
}
