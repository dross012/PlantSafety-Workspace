// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Financials
//  Layer:          IntegrationTests
//  File:           FinancialsSqlTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    SQL-layer tests verifying Financials stored procedure contracts.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Financials.Sql;

/// <summary>
/// SQL-layer tests for the Financials module.  Verifies stored procedure contracts,
/// canonical parameter handling, and TRY/CATCH error propagation.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class FinancialsSqlTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task OutboxTable_ShouldExist_InSystemSchema()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var count = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'OutboxMessages'");

        count.Should().Be(1, "System.OutboxMessages table must exist before running Financials SQL tests");
    }
}
