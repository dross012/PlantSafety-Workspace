// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Employees
//  Layer:          Infrastructure
//  File:           EmployeesDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Employees read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.Employees.Models;
using PlantSafety.Application.CrossDomain.Employees.Repositories;

namespace PlantSafety.Infrastructure.CrossDomain.Employees.Persistence;

/// <summary>Dapper-backed repository for <see cref="EmployeesItemDto"/> queries.</summary>
public sealed class EmployeesDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<EmployeesDapperRepository> logger)
    : IEmployeesRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EmployeesItemDto>> GetEmployeesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEmployeesAsync for Employees tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EmployeesItemDto>(
            "[CrossDomain].[usp_Employees_GetEmployees]", parameters, cancellationToken);
    }
}
