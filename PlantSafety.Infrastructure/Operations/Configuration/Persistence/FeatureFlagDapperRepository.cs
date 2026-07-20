// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Configuration
//  Layer:          Infrastructure
//  File:           FeatureFlagDapperRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IFeatureFlagRepository. Reads/writes
//                  [System].[FeatureFlag] directly in each database in
//                  ConfigurationOptions.DatabaseConnectionStrings (currently ArchiveDB and
//                  IntegrationDB only — confirmed by reading the full 144-feature-flags.sql
//                  migration). Server-level per-database platform state, not tenant-scoped
//                  RLS data, so this repository opens its own SqlConnection per database
//                  key rather than going through ISqlConnectionFactory/DapperQueryExecutor/
//                  DapperCommandExecutor (same reasoning as Operations.SchemaGuard/
//                  Archival/Backup). SqlException handling on the write path routes
//                  through the shared ErrorClassifier so failures surface via the
//                  platform's existing exception taxonomy. Toggling IsEnabled is a normal,
//                  reversible UPDATE — no destructive-action guardrails are required, but
//                  every toggle is written with the caller's real identity in UpdatedBy
//                  (never SYSTEM_USER) and is logged by the calling handler.
//                  Additive-only evolution.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Configuration.Repositories;
using PlantSafety.Infrastructure.Common.Exceptions;
using PlantSafety.Infrastructure.Operations.Configuration.Options;
using PlantSafety.SharedKernel.Exceptions;

namespace PlantSafety.Infrastructure.Operations.Configuration.Persistence;

/// <summary>Dapper-backed repository over the real Phase 14 Governance [System].[FeatureFlag] tables.</summary>
public sealed class FeatureFlagDapperRepository : IFeatureFlagRepository
{
    private const string SelectAllSql = @"
SELECT FlagName, IsEnabled, Description, UpdatedUtc, UpdatedBy
FROM [System].[FeatureFlag]
ORDER BY FlagName ASC;";

    private const string UpdateSql = @"
UPDATE [System].[FeatureFlag]
SET IsEnabled = @IsEnabled,
    UpdatedUtc = SYSUTCDATETIME(),
    UpdatedBy = @UpdatedBy
OUTPUT INSERTED.FlagName, INSERTED.IsEnabled, INSERTED.Description, INSERTED.UpdatedUtc, INSERTED.UpdatedBy
WHERE FlagName = @FlagName;";

    private readonly ConfigurationOptions _options;
    private readonly ILogger<FeatureFlagDapperRepository> _logger;

    /// <summary>Initialises dependencies.</summary>
    public FeatureFlagDapperRepository(IOptions<ConfigurationOptions> options, ILogger<FeatureFlagDapperRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetDatabaseKeys() => _options.DatabaseConnectionStrings.Keys.ToList();

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FeatureFlagDto>> GetAllAsync(string databaseKey, CancellationToken cancellationToken)
    {
        var connectionString = ResolveConnectionString(databaseKey);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<FeatureFlagRow>(
            new CommandDefinition(SelectAllSql, cancellationToken: cancellationToken));

        return rows.Select(r => ToDto(databaseKey, r)).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FeatureFlagDto>> GetAllAcrossDatabasesAsync(CancellationToken cancellationToken)
    {
        var all = new List<FeatureFlagDto>();

        foreach (var databaseKey in GetDatabaseKeys())
        {
            try
            {
                var flags = await GetAllAsync(databaseKey, cancellationToken);
                all.AddRange(flags);
            }
            catch (SqlException ex)
            {
                // One database being unreachable should not blank out the whole dashboard --
                // log and continue with whatever databases succeeded.
                _logger.LogError(ex, "Failed to retrieve feature flags from database {DatabaseKey} (SQL error {ErrorNumber}); continuing with remaining databases.", databaseKey, ex.Number);
            }
        }

        return all;
    }

    /// <inheritdoc/>
    public async Task<FeatureFlagDto> SetEnabledAsync(string databaseKey, string flagName, bool isEnabled, string updatedBy, CancellationToken cancellationToken)
    {
        var connectionString = ResolveConnectionString(databaseKey);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        try
        {
            var row = await connection.QuerySingleOrDefaultAsync<FeatureFlagRow>(
                new CommandDefinition(
                    UpdateSql,
                    new { FlagName = flagName, IsEnabled = isEnabled, UpdatedBy = updatedBy },
                    cancellationToken: cancellationToken));

            if (row is null)
            {
                throw new NotFoundException("System.FeatureFlag", flagName);
            }

            return ToDto(databaseKey, row);
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "System.FeatureFlag.Update", _logger);
        }
    }

    private string ResolveConnectionString(string databaseKey)
    {
        if (!_options.DatabaseConnectionStrings.TryGetValue(databaseKey, out var connectionString))
        {
            throw new ArgumentException(
                $"Unknown database key '{databaseKey}'. Known keys: {string.Join(", ", GetDatabaseKeys())}.",
                nameof(databaseKey));
        }

        return connectionString;
    }

    private static FeatureFlagDto ToDto(string databaseKey, FeatureFlagRow row) =>
        new(databaseKey, row.FlagName, row.IsEnabled, row.Description, row.UpdatedUtc, row.UpdatedBy);

    /// <summary>Raw Dapper projection matching [System].[FeatureFlag]'s selected columns.</summary>
    private sealed class FeatureFlagRow
    {
        public string FlagName { get; init; } = string.Empty;
        public bool IsEnabled { get; init; }
        public string? Description { get; init; }
        public DateTime UpdatedUtc { get; init; }
        public string UpdatedBy { get; init; } = string.Empty;
    }
}
