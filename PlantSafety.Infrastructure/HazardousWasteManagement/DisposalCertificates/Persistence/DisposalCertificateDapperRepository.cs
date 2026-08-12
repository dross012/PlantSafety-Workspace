// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Infrastructure
//  File:           DisposalCertificateDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Dapper repository for the DisposalCertificate aggregate. Implements both the
//                  Domain-layer write repository and the Application-layer read-model
//                  repository against the same table. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Specifications;
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.ValueObjects.Ids;
using PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Aggregates.DisposalCertificate;
using IWriteRepo = PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Interfaces.IDisposalCertificateRepository;
using IReadRepo = PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Repositories.IDisposalCertificateRepository;

namespace PlantSafety.Infrastructure.HazardousWasteManagement.DisposalCertificates.Persistence;

/// <summary>Flat Dapper row for [HazardousWasteManagement].[DisposalCertificates], column-for-column with the DisposalCertificate aggregate.</summary>
internal sealed class DisposalCertificateRow
{
    public Guid DisposalCertificateId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid ManifestId { get; init; }
    public Guid TsdfFacilityId { get; init; }
    public string? DisposalMethod { get; init; }
    public DateTime? DisposalDateUtc { get; init; }
    public DateTime? CertificateReceivedAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Flat Dapper row for the WasteStreamId-joined disposal history read model.</summary>
internal sealed class DisposalCertificateHistoryRow
{
    public Guid DisposalCertificateId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public Guid WasteStreamId { get; init; }
    public Guid ManifestId { get; init; }
    public Guid TsdfFacilityId { get; init; }
    public string? DisposalMethod { get; init; }
    public DateTime? DisposalDateUtc { get; init; }
    public DateTime? CertificateReceivedAtUtc { get; init; }
    public string? Status { get; init; }
}

/// <summary>Dapper-backed repository for the DisposalCertificate aggregate.</summary>
public sealed class DisposalCertificateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "HazardousWasteManagement";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(DisposalCertificateId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("DisposalCertificateId", id.Value).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<DisposalCertificateRow>(
            $"[{Schema}].[usp_DisposalCertificates_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, DisposalCertificateRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(DisposalCertificateSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<DisposalCertificateRow>(
            $"[{Schema}].[usp_DisposalCertificates_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, DisposalCertificateRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("DisposalCertificateId", aggregate, "DisposalCertificateId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ManifestId", aggregate, "ManifestId")
            .AddAggregateProperty("TsdfFacilityId", aggregate, "TsdfFacilityId")
            .AddAggregateProperty("DisposalMethod", aggregate, "DisposalMethod")
            .AddAggregateProperty("DisposalDateUtc", aggregate, "DisposalDateUtc")
            .AddAggregateProperty("CertificateReceivedAtUtc", aggregate, "CertificateReceivedAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_DisposalCertificates_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("DisposalCertificateId", aggregate, "DisposalCertificateId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_DisposalCertificates_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetDisposalHistoryByStreamAsync"/>
    public async Task<IReadOnlyList<DisposalCertificateItemDto>> GetDisposalHistoryByStreamAsync(
        Guid tenantId, Guid wasteStreamId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("WasteStreamId", wasteStreamId)
            .Build();

        var rows = await queryExecutor.QueryAsync<DisposalCertificateHistoryRow>(
            $"[{Schema}].[usp_DisposalCertificates_GetHistoryByStream]", parameters, cancellationToken);

        return rows.Select(row => new DisposalCertificateItemDto
        {
            DisposalCertificateId = row.DisposalCertificateId,
            TenantId = row.TenantId,
            SiteId = row.SiteId,
            ManifestId = row.ManifestId,
            WasteStreamId = row.WasteStreamId,
            TsdfFacilityId = row.TsdfFacilityId,
            DisposalMethod = row.DisposalMethod ?? string.Empty,
            DisposalDateUtc = row.DisposalDateUtc ?? default,
            CertificateReceivedAtUtc = row.CertificateReceivedAtUtc ?? default,
            Status = row.Status ?? string.Empty,
        }).ToList();
    }
}
