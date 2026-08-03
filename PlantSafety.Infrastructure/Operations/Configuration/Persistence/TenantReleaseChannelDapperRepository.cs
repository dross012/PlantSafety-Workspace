// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Configuration
//  Layer:          Infrastructure
//  File:           TenantReleaseChannelDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of ITenantReleaseChannelRepository.
//                  Reads/writes [Release].[TenantReleaseChannel] in PlantSafety.ArchiveDB
//                  (Phase 7 §7.2, migration 200-tenant-user-region-feature-flag-overrides.sql).
//                  Additive-only evolution.
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

/// <summary>Dapper-backed repository over the real [Release].[TenantReleaseChannel] table.</summary>
public sealed class TenantReleaseChannelDapperRepository : ITenantReleaseChannelRepository
{
    private const string ArchiveDbKey = "ArchiveDB";

    private const string FindSql = @"
SELECT TenantId, Channel, UpdatedUtc, UpdatedBy
FROM [Release].[TenantReleaseChannel]
WHERE TenantId = @TenantId;";

    private const string UpsertSql = @"
MERGE [Release].[TenantReleaseChannel] AS tgt
USING (SELECT @TenantId AS TenantId) AS src
ON tgt.TenantId = src.TenantId
WHEN MATCHED THEN
    UPDATE SET Channel = @Channel, UpdatedUtc = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
WHEN NOT MATCHED BY TARGET THEN
    INSERT (TenantId, Channel, UpdatedUtc, UpdatedBy)
    VALUES (@TenantId, @Channel, SYSUTCDATETIME(), @UpdatedBy)
OUTPUT INSERTED.TenantId, INSERTED.Channel, INSERTED.UpdatedUtc, INSERTED.UpdatedBy;";

    private readonly ConfigurationOptions _options;
    private readonly ILogger<TenantReleaseChannelDapperRepository> _logger;

    /// <summary>Initialises dependencies.</summary>
    public TenantReleaseChannelDapperRepository(IOptions<ConfigurationOptions> options, ILogger<TenantReleaseChannelDapperRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<TenantReleaseChannelDto?> FindAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ResolveConnectionString());
        await connection.OpenAsync(cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<Row>(
            new CommandDefinition(FindSql, new { TenantId = tenantId }, cancellationToken: cancellationToken));

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<TenantReleaseChannelDto> SetAsync(Guid tenantId, TenantReleaseChannel channel, string updatedBy, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ResolveConnectionString());
        await connection.OpenAsync(cancellationToken);

        try
        {
            var row = await connection.QuerySingleAsync<Row>(
                new CommandDefinition(
                    UpsertSql,
                    new { TenantId = tenantId, Channel = channel.ToString(), UpdatedBy = updatedBy },
                    cancellationToken: cancellationToken));

            return ToDto(row);
        }
        catch (SqlException ex)
        {
            throw ErrorClassifier.Classify(ex, "Release.TenantReleaseChannel.Set", _logger);
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

    private static TenantReleaseChannelDto ToDto(Row row) =>
        new(row.TenantId, Enum.Parse<TenantReleaseChannel>(row.Channel), row.UpdatedUtc, row.UpdatedBy);

    /// <summary>Raw Dapper projection matching [Release].[TenantReleaseChannel]'s selected columns.</summary>
    private sealed class Row
    {
        public Guid TenantId { get; init; }
        public string Channel { get; init; } = string.Empty;
        public DateTime UpdatedUtc { get; init; }
        public string UpdatedBy { get; init; } = string.Empty;
    }
}
