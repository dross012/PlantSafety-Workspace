// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Infrastructure
//  File:           PublicSafetyAgencyDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPublicSafetyAgencyRepository (Domain
//                  repository interface for the PublicSafetyAgency aggregate root). All DB access
//                  via stored procedures in the [PublicSafetyCommunication] schema. Aggregate
//                  reconstruction uses AggregateReflectionHydrator because PublicSafetyAgency
//                  exposes only a private rehydration constructor and private-setter properties.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Entities;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Events;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Interfaces;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Specifications;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.PublicSafetyCommunication.AgencyDirectory.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PublicSafetyAgency"/> aggregate root.</summary>
public sealed class PublicSafetyAgencyDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PublicSafetyAgencyDapperRepository> logger)
    : IPublicSafetyAgencyRepository
{
    /// <inheritdoc/>
    public async Task<PublicSafetyAgency?> GetByIdAsync(PublicSafetyAgencyId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PublicSafetyAgency {PublicSafetyAgencyId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("PublicSafetyAgencyId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PublicSafetyAgencyRow>(
            "[PublicSafetyCommunication].[usp_PublicSafetyAgency_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        var contactRows = await queryExecutor.QueryAsync<AgencyContactRow>(
            "[PublicSafetyCommunication].[usp_AgencyContact_GetByAgencyId]", parameters, cancellationToken);

        return Hydrate(row, contactRows);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PublicSafetyAgency>> ListAsync(PublicSafetyAgencySpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List PublicSafetyAgency via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PublicSafetyAgencyRow>(
            "[PublicSafetyCommunication].[usp_PublicSafetyAgency_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PublicSafetyAgency>();

        foreach (var row in rows)
        {
            var contactParams = new StoredProcedureBuilder().AddGuid("PublicSafetyAgencyId", row.PublicSafetyAgencyId).Build();
            var contactRows = await queryExecutor.QueryAsync<AgencyContactRow>(
                "[PublicSafetyCommunication].[usp_AgencyContact_GetByAgencyId]", contactParams, cancellationToken);

            var aggregate = Hydrate(row, contactRows);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PublicSafetyAgency aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PublicSafetyAgency {PublicSafetyAgencyId}", aggregate.PublicSafetyAgencyId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PublicSafetyAgencyId", aggregate.PublicSafetyAgencyId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("AgencyName", aggregate.AgencyName, 400)
            .AddString("AgencyType", aggregate.AgencyType.ToString(), 64)
            .AddString("JurisdictionDescription", aggregate.JurisdictionDescription, 400)
            .AddString("PrimaryPhoneNumber", aggregate.PrimaryPhoneNumber, 64)
            .AddString("PrimaryEmail", aggregate.PrimaryEmail, 320)
            .AddString("VerificationStatus", aggregate.VerificationStatus.ToString(), 50)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PublicSafetyCommunication].[usp_PublicSafetyAgency_Insert]", parameters, tx, cancellationToken: cancellationToken);

        foreach (var contact in aggregate.Contacts)
        {
            var contactParameters = new StoredProcedureBuilder()
                .AddGuid("AgencyContactId", contact.AgencyContactId.Value)
                .AddGuid("PublicSafetyAgencyId", aggregate.PublicSafetyAgencyId.Value)
                .AddString("Role", contact.Role.ToString(), 64)
                .AddString("ContactName", contact.ContactName, 400)
                .AddString("PhoneNumber", contact.PhoneNumber, 64)
                .AddString("Email", contact.Email, 320)
                .AddDateTime("CreatedAtUtc", contact.CreatedAtUtc.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[PublicSafetyCommunication].[usp_AgencyContact_Insert]", contactParameters, tx, cancellationToken: cancellationToken);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PublicSafetyAgency aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PublicSafetyAgency {PublicSafetyAgencyId}", aggregate.PublicSafetyAgencyId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PublicSafetyAgencyId", aggregate.PublicSafetyAgencyId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("AgencyName", aggregate.AgencyName, 400)
            .AddString("AgencyType", aggregate.AgencyType.ToString(), 64)
            .AddString("JurisdictionDescription", aggregate.JurisdictionDescription, 400)
            .AddString("PrimaryPhoneNumber", aggregate.PrimaryPhoneNumber, 64)
            .AddString("PrimaryEmail", aggregate.PrimaryEmail, 320)
            .AddString("VerificationStatus", aggregate.VerificationStatus.ToString(), 50)
            .AddDateTime("LastVerifiedAtUtc", aggregate.LastVerifiedAtUtc?.Value)
            .AddDateTime("NextVerificationDueAtUtc", aggregate.NextVerificationDueAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[PublicSafetyCommunication].[usp_PublicSafetyAgency_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"PublicSafetyAgency {aggregate.PublicSafetyAgencyId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PublicSafetyAgency"/> aggregate from its flat read-model row and contact rows.</summary>
    private static PublicSafetyAgency Hydrate(PublicSafetyAgencyRow row, IEnumerable<AgencyContactRow> contactRows)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PublicSafetyAgency>();

        AggregateReflectionHydrator.SetProperty(agg, "PublicSafetyAgencyId", PublicSafetyAgencyId.FromGuid(row.PublicSafetyAgencyId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<AgencyOperationalStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "AgencyName", row.AgencyName);
        AggregateReflectionHydrator.SetProperty(agg, "AgencyType", Enum.Parse<PublicSafetyAgencyType>(row.AgencyType));
        AggregateReflectionHydrator.SetProperty(agg, "JurisdictionDescription", row.JurisdictionDescription);
        AggregateReflectionHydrator.SetProperty(agg, "PrimaryPhoneNumber", row.PrimaryPhoneNumber);
        AggregateReflectionHydrator.SetProperty(agg, "PrimaryEmail", row.PrimaryEmail);
        AggregateReflectionHydrator.SetProperty(agg, "VerificationStatus", Enum.Parse<AgencyVerificationStatus>(row.VerificationStatus));
        AggregateReflectionHydrator.SetProperty(agg, "LastVerifiedAtUtc", row.LastVerifiedAtUtc.HasValue ? UtcTimestamp.From(row.LastVerifiedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "NextVerificationDueAtUtc", row.NextVerificationDueAtUtc.HasValue ? UtcTimestamp.From(row.NextVerificationDueAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPublicSafetyAgencyDomainEvent>());

        var contacts = contactRows
            .Select(c => new AgencyContact(
                AgencyContactId.FromGuid(c.AgencyContactId),
                PublicSafetyAgencyId.FromGuid(c.PublicSafetyAgencyId),
                Enum.Parse<AgencyContactRole>(c.Role),
                c.ContactName,
                c.PhoneNumber,
                c.Email,
                UtcTimestamp.From(c.CreatedAtUtc)))
            .ToList();

        AggregateReflectionHydrator.SetField(agg, "_contacts", contacts);

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PublicSafetyAgency_GetById</c>/<c>usp_PublicSafetyAgency_GetAllActive</c>.</summary>
    private sealed record PublicSafetyAgencyRow
    {
        /// <summary>Optimistic-concurrency token (native RowVersion projected as a hex string).</summary>
        public string? ETag { get; init; }

        public required Guid PublicSafetyAgencyId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string AgencyName { get; init; }
        public required string AgencyType { get; init; }
        public required string JurisdictionDescription { get; init; }
        public required string PrimaryPhoneNumber { get; init; }
        public string? PrimaryEmail { get; init; }
        public required string VerificationStatus { get; init; }
        public DateTime? LastVerifiedAtUtc { get; init; }
        public DateTime? NextVerificationDueAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }

    /// <summary>Flat row shape returned by <c>usp_AgencyContact_GetByAgencyId</c>.</summary>
    private sealed record AgencyContactRow
    {
        public required Guid AgencyContactId { get; init; }
        public required Guid PublicSafetyAgencyId { get; init; }
        public required string Role { get; init; }
        public required string ContactName { get; init; }
        public required string PhoneNumber { get; init; }
        public string? Email { get; init; }
        public DateTime? LastVerifiedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
    }
}
