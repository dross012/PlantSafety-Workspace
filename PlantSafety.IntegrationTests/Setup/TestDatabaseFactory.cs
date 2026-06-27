// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           TestDatabaseFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Creates and migrates the integration test SQL Server database schema.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Setup;

/// <summary>
/// Applies all canonical System schema objects (OutboxMessages, ProcessedEvents)
/// against the provided Testcontainers connection string before tests execute.
/// </summary>
public static class TestDatabaseFactory
{
    /// <summary>Creates System schema and required tables.</summary>
    public static async Task ApplySchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
                EXEC('CREATE SCHEMA [System]');

            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                           WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'OutboxMessages')
            BEGIN
                CREATE TABLE [System].[OutboxMessages] (
                    MessageId     UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
                    EventType     NVARCHAR(200)    NOT NULL,
                    SchemaVersion NVARCHAR(20)     NOT NULL DEFAULT '2026.1',
                    TenantId      UNIQUEIDENTIFIER NOT NULL,
                    ActorId       UNIQUEIDENTIFIER NOT NULL,
                    CorrelationId UNIQUEIDENTIFIER NOT NULL,
                    OccurredAt    DATETIME2(3)     NOT NULL,
                    Payload       NVARCHAR(MAX)    NOT NULL,
                    Processed     BIT              NOT NULL DEFAULT 0,
                    ProcessedAt   DATETIME2(3)         NULL
                );
            END;

            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                           WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'ProcessedEvents')
            BEGIN
                CREATE TABLE [System].[ProcessedEvents] (
                    EventId     UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    ProcessedAt DATETIME2(3)     NOT NULL
                );
            END;");
    }
}
