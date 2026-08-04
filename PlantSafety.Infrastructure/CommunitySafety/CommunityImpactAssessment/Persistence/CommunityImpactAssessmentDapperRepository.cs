// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityImpactAssessment
//  Layer:          Infrastructure
//  File:           CommunityImpactAssessmentDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CommunityImpactAssessment aggregate. Implements both
//                  the Domain-layer write repository and the Application-layer read-model
//                  repository (by-incident lookup). All DB access via stored procedures; no
//                  dynamic SQL. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.ValueObjects.Ids;
using PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Aggregates.CommunityImpactAssessment;
using IWriteRepo = PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Interfaces.ICommunityImpactAssessmentRepository;
using IReadRepo = PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Repositories.ICommunityImpactAssessmentRepository;

namespace PlantSafety.Infrastructure.CommunitySafety.CommunityImpactAssessment.Persistence;

/// <summary>Flat Dapper row for [CommunitySafety].[CommunityImpactAssessment], column-for-column with the aggregate.</summary>
internal sealed class CommunityImpactAssessmentRow
{
    public Guid CommunityImpactAssessmentId { get; init; }
    public Guid TenantId { get; init; }
    public Guid SiteId { get; init; }
    public string? Status { get; init; }
    public Guid IncidentId { get; init; }
    public string? Severity { get; init; }
    public string? AffectedArea { get; init; }
    public int? EstimatedPersonsAffected { get; init; }
    public string? Description { get; init; }
    public bool RequiresRightToKnowReporting { get; init; }
    public Guid AssessedByUserId { get; init; }
    public DateTime? AssessedAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
    public DateTime? FinalizedAtUtc { get; init; }
}

/// <summary>
/// Dapper-backed repository for the CommunityImpactAssessment aggregate. Implements the Domain
/// write repository and the Application read-model repository against the same table.
/// </summary>
public sealed class CommunityImpactAssessmentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CommunityImpactAssessmentDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "CommunitySafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(CommunityImpactAssessmentId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for CommunityImpactAssessment {CommunityImpactAssessmentId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CommunityImpactAssessmentId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<CommunityImpactAssessmentRow>(
            $"[{Schema}].[usp_CommunityImpactAssessment_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, CommunityImpactAssessmentRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for CommunityImpactAssessment {CommunityImpactAssessmentId}", aggregate.CommunityImpactAssessmentId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("CommunityImpactAssessmentId", aggregate, "CommunityImpactAssessmentId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("IncidentId", aggregate, "IncidentId")
            .AddAggregateProperty("Severity", aggregate, "Severity")
            .AddAggregateProperty("AffectedArea", aggregate, "AffectedArea")
            .AddAggregateProperty("EstimatedPersonsAffected", aggregate, "EstimatedPersonsAffected")
            .AddAggregateProperty("Description", aggregate, "Description")
            .AddAggregateProperty("RequiresRightToKnowReporting", aggregate, "RequiresRightToKnowReporting")
            .AddAggregateProperty("AssessedByUserId", aggregate, "AssessedByUserId")
            .AddAggregateProperty("AssessedAtUtc", aggregate, "AssessedAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_CommunityImpactAssessment_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for CommunityImpactAssessment {CommunityImpactAssessmentId}", aggregate.CommunityImpactAssessmentId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("CommunityImpactAssessmentId", aggregate, "CommunityImpactAssessmentId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("FinalizedAtUtc", aggregate, "FinalizedAtUtc")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_CommunityImpactAssessment_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetImpactAssessmentItemsByIncidentAsync"/>
    public async Task<IReadOnlyList<CommunityImpactAssessmentItemDto>> GetImpactAssessmentItemsByIncidentAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetImpactAssessmentItemsByIncidentAsync for Incident {IncidentId}", incidentId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("IncidentId", incidentId)
            .Build();

        return await queryExecutor.QueryAsync<CommunityImpactAssessmentItemDto>(
            $"[{Schema}].[usp_CommunityImpactAssessment_GetByIncident]", parameters, cancellationToken);
    }
}
