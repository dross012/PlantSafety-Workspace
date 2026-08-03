// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Configuration
//  Layer:          Infrastructure
//  File:           TenantFeatureFlagOverrideDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of ITenantFeatureFlagOverrideRepository.
//                  Reads/writes [Release].[TenantFeatureFlagOverride] in
//                  PlantSafety.ArchiveDB (Phase 7, migration
//                  200-tenant-user-region-feature-flag-overrides.sql). Same connection
//                  strategy as FeatureFlagDapperRepository -- opens its own SqlConnection
//                  against the "ArchiveDB" key in ConfigurationOptions rather than going
//                  through ISqlConnectionFactory, since this is server-level platform
//                  configuration, not tenant-scoped RLS data. Additive-only evolution.
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

/// <summary>Dapper-backed repository over the real [Release].[TenantFeatureFlagOverride] table.</summary>
public sealed class TenantFeatureFlagOverrideDapperRepository : ITenantFeatureFlagOverrideRepository
{
    private const string ArchiveDbKey = "ArchiveDB";

    private const string FindSql = @"
SELECT TenantId, FeatureName, Enabled, RolloutPercentage, UpdatedUtc, UpdatedBy
FROM [Release].[TenantFeatureFlagOverride]
WHERE TenantId = @TenantId AND FeatureName = @FeatureName;";

    private const string GetAllForFeatureSql = @"
SELECT TenantId, FeatureName, Enabled, RolloutPercentage, UpdatedUtc, UpdatedBy
FROM [Release].[TenantFeatureFlagOverride]
WHERE FeatureName = @FeatureName
ORDER BY TenantId ASC;";

    private const string UpsertSql = @"
MERGE [Release].[TenantFeatureFlagOverride] AS tgt
USING (SELECT @TenantId AS TenantId, @FeatureName AS FeatureName) AS src
ON tgt.TenantId = src.TenantId AND tgt.FeatureName = src.FeatureName
WHEN MATCHED THEN
    UPDATE SET Enabled = @Enabled, RolloutPercentage = @RolloutPercentage, UpdatedUtc = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
WHEN NOT MATCHED BY TARGET THEN
    INSERT (TenantId, FeatureName, Enabled, RolloutPercentage, UpdatedUtc, UpdatedBy)
    VALUES (@TenantId, @FeatureName, @Enabled, @RolloutPercentage, SYSUTCDATETIME(), @UpdatedBy)
OUTPUT INSERTED.TenantId, INSERTED.FeatureName, INSERTED.Enabled, INSERTED.RolloutPercentage, INSERTED.UpdatedUtc, INSERTED.UpdatedBy;";

    private const string ResetRolloutForFeatureSql = @"
UPDATE [Release].[TenantFeatureFlagOverride]
SET RolloutPercentage = 0, UpdatedUtc = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
WHERE FeatureName = @FeatureName AND RolloutPercentage IS NOT NULL AND RolloutPercentage <> 0;";

    private readonly ConfigurationOptions _options;
    private readonly ILogger<TenantFeatureFlagOverrideDapperRepository> _logger;

    /// <summary>Initialises dependencies.</summary>
    public TenantFeatureFlagOverrideDapperRepository(IOptions<ConfigurationOptions> options, ILogger<TenantFeatureFlagOverrideDapperRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<TenantFeatureFlagOverrideDto?> FindAsync(Guid tenantId, string featureName, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ResolveConnectionString());
        await connection.OpenAsync(cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<Row>(
            new CommandDefinition(FindSql, new { TenantId = tenantId, FeatureName = featureName }, cancellationToken: cancellationToken));

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TenantFeatureFlagOverrideDto>> GetAllForFeatureAsync(string featureName, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ResolveConnectionString());
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<Row>(
            new CommandDefinition(GetAllForFeatureSql, new { FeatureName = featureName }, cancellationToken: cancellationToken));

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<TenantFeatureFlagOverrideDto> SetAsync(Guid tenantId, string featureName, bool enabled, int? rolloutPercentage, string updatedBy, CancellationToken cancellationToken)
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
                    new { TenantId = tenantId, FeatureName = featureName, Enabled = enabled, RolloutPercentage = rolloutPercentage, UpdatedBy = updatedBy },
                    cancellationToken: cancellationToken));

            return ToDto(row);
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "Release.TenantFeatureFlagOverride.Set", _logger);
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
            throw ErrorClassifier.Classify(ex, "Release.TenantFeatureFlagOverride.ResetRollout", _logger);
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

    private static TenantFeatureFlagOverrideDto ToDto(Row row) =>
        new(row.TenantId, row.FeatureName, row.Enabled, row.RolloutPercentage, row.UpdatedUtc, row.UpdatedBy);

    /// <summary>Raw Dapper projection matching [Release].[TenantFeatureFlagOverride]'s selected columns.</summary>
    private sealed class Row
    {
        public Guid TenantId { get; init; }
        public string FeatureName { get; init; } = string.Empty;
        public bool Enabled { get; init; }
        public int? RolloutPercentage { get; init; }
        public DateTime UpdatedUtc { get; init; }
        public string UpdatedBy { get; init; } = string.Empty;
    }
}
