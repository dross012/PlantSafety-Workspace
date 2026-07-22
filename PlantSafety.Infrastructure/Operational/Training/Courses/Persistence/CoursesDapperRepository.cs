// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Courses
//  Layer:          Infrastructure
//  File:           CoursesDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Courses read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.Training.Courses.Models;
using PlantSafety.Application.Operational.Training.Courses.Repositories;

namespace PlantSafety.Infrastructure.Operational.Training.Courses.Persistence;

/// <summary>Dapper-backed repository for <see cref="CourseItemDto"/> queries.</summary>
public sealed class CoursesDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<CoursesDapperRepository> logger)
    : ICoursesRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<CourseItemDto>> GetCoursesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetCoursesAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<CourseItemDto>(
            "[Operational].[usp_Courses_List]", parameters, cancellationToken);
    }
}
