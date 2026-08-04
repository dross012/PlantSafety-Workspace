// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Transporters
//  Layer:          Infrastructure
//  File:           TransporterDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Dapper repository for the Transporter aggregate root. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Transporters.Specifications;
using PlantSafety.Domain.HazardousWasteManagement.Transporters.ValueObjects.Ids;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.HazardousWasteManagement.Transporters.Aggregates.Transporter;
using IWriteRepo = PlantSafety.Domain.HazardousWasteManagement.Transporters.Interfaces.ITransporterRepository;

namespace PlantSafety.Infrastructure.HazardousWasteManagement.Transporters.Persistence;

/// <summary>Flat Dapper row for [HazardousWasteManagement].[Transporters], column-for-column with the Transporter aggregate.</summary>
internal sealed class TransporterRow
{
    public Guid TransporterId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? CompanyName { get; init; }
    public string? EpaTransporterId { get; init; }
    public DateTime? LicenseExpiresAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Dapper-backed repository for the Transporter aggregate. Integrates with
/// Infrastructure.Integrations for transporter EDI hand-off (future extension point).</summary>
public sealed class TransporterDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<TransporterDapperRepository> logger)
    : IWriteRepo
{
    private const string Schema = "HazardousWasteManagement";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(TransporterId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TransporterId", id.Value).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<TransporterRow>(
            $"[{Schema}].[usp_Transporters_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, TransporterRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(TransporterSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<TransporterRow>(
            $"[{Schema}].[usp_Transporters_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, TransporterRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("TransporterId", aggregate, "TransporterId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("CompanyName", aggregate, "CompanyName")
            .AddAggregateProperty("EpaTransporterId", aggregate, "EpaTransporterId")
            .AddAggregateProperty("LicenseExpiresAtUtc", aggregate, "LicenseExpiresAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_Transporters_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("TransporterId", aggregate, "TransporterId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_Transporters_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }
}
