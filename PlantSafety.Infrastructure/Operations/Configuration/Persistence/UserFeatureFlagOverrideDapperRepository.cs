// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Configuration
//  Layer:          Infrastructure
//  File:           UserFeatureFlagOverrideDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IUserFeatureFlagOverrideRepository.
//                  Reads/writes [Release].[UserFeatureFlagOverride] in
//                  PlantSafety.ArchiveDB (Phase 7, migration
//                  200-tenant-user-region-feature-flag-overrides.sql). Additive-only evolution.
// ============================================================================
using System;
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

/// <summary>Dapper-backed repository over the real [Release].[UserFeatureFlagOverride] table.</summary>
public sealed class UserFeatureFlagOverrideDapperRepository : IUserFeatureFlagOverrideRepository
{
    private const string ArchiveDbKey = "ArchiveDB";

    private const string FindSql = @"
SELECT UserId, FeatureName, Enabled, UpdatedUtc, UpdatedBy
FROM [Release].[UserFeatureFlagOverride]
WHERE UserId = @UserId AND FeatureName = @FeatureName;";

    private const string UpsertSql = @"
MERGE [Release].[UserFeatureFlagOverride] AS tgt
USING (SELECT @UserId AS UserId, @FeatureName AS FeatureName) AS src
ON tgt.UserId = src.UserId AND tgt.FeatureName = src.FeatureName
WHEN MATCHED THEN
    UPDATE SET Enabled = @Enabled, UpdatedUtc = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
WHEN NOT MATCHED BY TARGET THEN
    INSERT (UserId, FeatureName, Enabled, UpdatedUtc, UpdatedBy)
    VALUES (@UserId, @FeatureName, @Enabled, SYSUTCDATETIME(), @UpdatedBy)
OUTPUT INSERTED.UserId, INSERTED.FeatureName, INSERTED.Enabled, INSERTED.UpdatedUtc, INSERTED.UpdatedBy;";

    private readonly ConfigurationOptions _options;
    private readonly ILogger<UserFeatureFlagOverrideDapperRepository> _logger;

    /// <summary>Initialises dependencies.</summary>
    public UserFeatureFlagOverrideDapperRepository(IOptions<ConfigurationOptions> options, ILogger<UserFeatureFlagOverrideDapperRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<UserFeatureFlagOverrideDto?> FindAsync(Guid userId, string featureName, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ResolveConnectionString());
        await connection.OpenAsync(cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<Row>(
            new CommandDefinition(FindSql, new { UserId = userId, FeatureName = featureName }, cancellationToken: cancellationToken));

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<UserFeatureFlagOverrideDto> SetAsync(Guid userId, string featureName, bool enabled, string updatedBy, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ResolveConnectionString());
        await connection.OpenAsync(cancellationToken);

        try
        {
            var row = await connection.QuerySingleAsync<Row>(
                new CommandDefinition(
                    UpsertSql,
                    new { UserId = userId, FeatureName = featureName, Enabled = enabled, UpdatedBy = updatedBy },
                    cancellationToken: cancellationToken));

            return ToDto(row);
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "Release.UserFeatureFlagOverride.Set", _logger);
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

    private static UserFeatureFlagOverrideDto ToDto(Row row) =>
        new(row.UserId, row.FeatureName, row.Enabled, row.UpdatedUtc, row.UpdatedBy);

    /// <summary>Raw Dapper projection matching [Release].[UserFeatureFlagOverride]'s selected columns.</summary>
    private sealed class Row
    {
        public Guid UserId { get; init; }
        public string FeatureName { get; init; } = string.Empty;
        public bool Enabled { get; init; }
        public DateTime UpdatedUtc { get; init; }
        public string UpdatedBy { get; init; } = string.Empty;
    }
}
