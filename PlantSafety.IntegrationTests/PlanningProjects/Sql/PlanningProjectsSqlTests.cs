// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PlanningProjects
//  Layer:          IntegrationTests
//  File:           PlanningProjectsSqlTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    SQL-layer tests for PlanningProjects stored procedure contracts.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.PlanningProjects.Sql;

/// <summary>SQL-layer tests for the PlanningProjects module.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class PlanningProjectsSqlTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task SystemSchema_ShouldExist_InDatabase()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM sys.schemas WHERE name = 'System'");

        count.Should().Be(1, "System schema must exist before running PlanningProjects SQL tests");
    }
}
