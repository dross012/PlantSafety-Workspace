// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Infrastructure
//  File:           TenantReadDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of ITenantReadRepository, projecting directly to
//                  TenantDto (no aggregate rehydration needed for reads). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Tenancy.Tenant.Models;
using PlantSafety.Application.Tenancy.Tenant.Repositories;

namespace PlantSafety.Infrastructure.Tenancy.Tenant.Persistence;

/// <summary>Dapper-backed implementation of <see cref="ITenantReadRepository"/>.</summary>
public sealed class TenantReadDapperRepository(DapperQueryExecutor queryExecutor) : ITenantReadRepository
{
    /// <inheritdoc/>
    public async Task<TenantDto?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<TenantRow>("[Tenancy].[usp_Tenant_GetById]", parameters, cancellationToken);
        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TenantDto>> ListAllAsync(CancellationToken cancellationToken)
    {
        var rows = await queryExecutor.QueryAsync<TenantRow>("[Tenancy].[usp_Tenant_ListAll]", new StoredProcedureBuilder().Build(), cancellationToken);
        return rows.Select(ToDto).ToList();
    }

    private static TenantDto ToDto(TenantRow row)
    {
        var configRow = string.IsNullOrWhiteSpace(row.ConfigurationJson)
            ? new TenantConfigurationRow([], [], [])
            : JsonSerializer.Deserialize<TenantConfigurationRow>(row.ConfigurationJson) ?? new TenantConfigurationRow([], [], []);

        return new TenantDto(
            row.TenantId, row.Name, row.Region, row.Status, row.PlanCode,
            new TenantConfigurationDto(
                configRow.EnabledModules,
                configRow.FeatureFlagOverrides.Select(f => new TenantFeatureFlagOverrideDto(f.FlagName, f.IsEnabled)).ToList(),
                configRow.CustomComplianceRuleCodes),
            row.CreatedAtUtc, row.ActivatedAtUtc, row.SuspendedAtUtc, row.SuspensionReason, row.ArchivedAtUtc, row.DeletedAtUtc);
    }

    private sealed record TenantRow
    {
        public required Guid TenantId { get; init; }
        public required string Name { get; init; }
        public required string Region { get; init; }
        public required string Status { get; init; }
        public required string PlanCode { get; init; }
        public string? ConfigurationJson { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public DateTime? ActivatedAtUtc { get; init; }
        public DateTime? SuspendedAtUtc { get; init; }
        public string? SuspensionReason { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public DateTime? DeletedAtUtc { get; init; }
    }

    private sealed record TenantConfigurationRow(
        List<string> EnabledModules,
        List<TenantFeatureFlagOverrideRow> FeatureFlagOverrides,
        List<string> CustomComplianceRuleCodes);

    private sealed record TenantFeatureFlagOverrideRow(string FlagName, bool IsEnabled);
}
