// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Configuration
//  Layer:          Infrastructure
//  File:           RegionFeatureFlagOverrideDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IRegionFeatureFlagOverrideRepository.
//                  Reads/writes [Release].[RegionFeatureFlagOverride] in
//                  PlantSafety.ArchiveDB (Phase 7, migration
//                  200-tenant-user-region-feature-flag-overrides.sql). Additive-only evolution.
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

namespace PlantSafety.Infrastructure.Operations.Configuration.Persistence;

/// <summary>Dapper-backed repository over the real [Release].[RegionFeatureFlagOverride] table.</summary>
public sealed class RegionFeatureFlagOverrideDapperRepository : IRegionFeatureFlagOverrideRepository
{
    private const string ArchiveDbKey = "ArchiveDB";

    private const string FindSql = @"
SELECT Region, FeatureName, Enabled, RolloutPercentage, UpdatedUtc, UpdatedBy
FROM [Release].[RegionFeatureFlagOverride]
WHERE Region = @Region AND FeatureName = @FeatureName;";

    private const string GetAllForFeatureSql = @"
SELECT Region, FeatureName, Enabled, RolloutPercentage, UpdatedUtc, UpdatedBy
FROM [Release].[RegionFeatureFlagOverride]
WHERE FeatureName = @FeatureName
ORDER BY Region ASC;";

    private const string UpsertSql = @"
MERGE [Release].[RegionFeatureFlagOverride] AS tgt
USING (SELECT @Region AS Region, @FeatureName AS FeatureName) AS src
ON tgt.Region = src.Region AND tgt.FeatureName = src.FeatureName
WHEN MATCHED THEN
    UPDATE SET Enabled = @Enabled, RolloutPercentage = @RolloutPercentage, UpdatedUtc = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Region, FeatureName, Enabled, RolloutPercentage, UpdatedUtc, UpdatedBy)
    VALUES (@Region, @FeatureName, @Enabled, @RolloutPercentage, SYSUTCDATETIME(), @UpdatedBy)
OUTPUT INSERTED.Region, INSERTED.FeatureName, INSERTED.Enabled, INSERTED.RolloutPercentage, INSERTED.UpdatedUtc, INSERTED.UpdatedBy;";

    private const string ResetRolloutForFeatureSql = @"
UPDATE [Release].[RegionFeatureFlagOverride]
SET RolloutPercentage = 0, UpdatedUtc = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
WHERE FeatureName = @FeatureName AND RolloutPercentage IS NOT NULL AND RolloutPercentage <> 0;";

    private readonly ConfigurationOptions _options;
    private readonly ILogger<RegionFeatureFlagOverrideDapperRepository> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegionFeatureFlagOverrideDapperRepository(IOptions<ConfigurationOptions> options, ILogger<RegionFeatureFlagOverrideDapperRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<RegionFeatureFlagOverrideDto?> FindAsync(string region, string featureName, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ResolveConnectionString());
        await connection.OpenAsync(cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<Row>(
            new CommandDefinition(FindSql, new { Region = region, FeatureName = featureName }, cancellationToken: cancellationToken));

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RegionFeatureFlagOverrideDto>> GetAllForFeatureAsync(string featureName, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ResolveConnectionString());
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<Row>(
            new CommandDefinition(GetAllForFeatureSql, new { FeatureName = featureName }, cancellationToken: cancellationToken));

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<RegionFeatureFlagOverrideDto> SetAsync(string region, string featureName, bool enabled, int? rolloutPercentage, string updatedBy, CancellationToken cancellationToken)
    {
        if (rolloutPercentage is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(rolloutPercentage), rolloutPercentage, "RolloutPercentage must be between 0 and 100, or null.");
        }

        await using var connection = new SqlConnection(ResolveConnectionString());
        await connection.OpenAsync(cancellationToken);

        try
        {
            var row = await connection.QuerySingleAsync<Row>(
                new CommandDefinition(
                    UpsertSql,
                    new { Region = region, FeatureName = featureName, Enabled = enabled, RolloutPercentage = rolloutPercentage, UpdatedBy = updatedBy },
                    cancellationToken: cancellationToken));

            return ToDto(row);
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "Release.RegionFeatureFlagOverride.Set", _logger);
        }
    }

    /// <inheritdoc/>
    public async Task<int> ResetRolloutPercentageForFeatureAsync(string featureName, string updatedBy, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ResolveConnectionString());
        await connection.OpenAsync(cancellationToken);

        try
        {
            return await connection.ExecuteAsync(
                new CommandDefinition(ResetRolloutForFeatureSql, new { FeatureName = featureName, UpdatedBy = updatedBy }, cancellationToken: cancellationToken));
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "Release.RegionFeatureFlagOverride.ResetRollout", _logger);
        }
    }

    private string ResolveConnectionString()
    {
        if (!_options.DatabaseConnectionStrings.TryGetValue(ArchiveDbKey, out var connectionString))
        {
            throw new ArgumentException($"Unknown database key '{ArchiveDbKey}'.", nameof(ArchiveDbKey));
        }

        return connectionString;
    }

    private static RegionFeatureFlagOverrideDto ToDto(Row row) =>
        new(row.Region, row.FeatureName, row.Enabled, row.RolloutPercentage, row.UpdatedUtc, row.UpdatedBy);

    /// <summary>Raw Dapper projection matching [Release].[RegionFeatureFlagOverride]'s selected columns.</summary>
    private sealed class Row
    {
        public string Region { get; init; } = string.Empty;
        public string FeatureName { get; init; } = string.Empty;
        public bool Enabled { get; init; }
        public int? RolloutPercentage { get; init; }
        public DateTime UpdatedUtc { get; init; }
        public string UpdatedBy { get; init; } = string.Empty;
    }
}
