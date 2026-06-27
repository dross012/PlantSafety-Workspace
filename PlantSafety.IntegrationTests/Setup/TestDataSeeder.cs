// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           TestDataSeeder.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Seeds minimal, deterministic, versioned test data before integration tests run.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Setup;

/// <summary>
/// Seeds all baseline test data into the Testcontainers database.
/// Data is minimal, deterministic, and versioned.  Never pulled from production.
/// </summary>
public static class TestDataSeeder
{
    /// <summary>Seeds all base test data required by the shared test host.</summary>
    public static async Task SeedAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        // Module seeders are added here as each module's schema is applied.
        // Pattern: await AlertsSeeder.SeedAsync(conn, cancellationToken);
        await Task.CompletedTask;
    }
}
