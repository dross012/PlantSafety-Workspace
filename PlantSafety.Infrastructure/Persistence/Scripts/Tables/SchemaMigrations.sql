-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           SchemaMigrations.sql
--  Created:        2026-06-17
--  Updated:        2026-07-19 (Schema Sentinel — finished stub, governance finding #1)
--  Author:         David (Ted) + Copilot
--  SchemaVersion:  2026.1
--  Description:    Migration history. Tracks all applied DB schema migrations.
--                  Additive-only, append-only, immutable audit/tracking table
--                  (metadata, not application data — never soft-deleted, never
--                  updated after insert except the best-effort Success/Notes
--                  fields set at apply time).
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
    EXEC('CREATE SCHEMA [System]');
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'SchemaMigrations')
BEGIN
    CREATE TABLE [System].[SchemaMigrations] (
        -- StrongId clustered PK
        MigrationId     UNIQUEIDENTIFIER NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_MigrationId DEFAULT NEWSEQUENTIALID(),

        -- Identity of the applied script
        ScriptName      NVARCHAR(260)    NOT NULL,
        Phase           NVARCHAR(100)    NULL,

        -- Apply-time metadata (canonical audit convention)
        AppliedAtUtc    DATETIME2(7)     NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedAtUtc DEFAULT SYSUTCDATETIME(),
        AppliedBy       NVARCHAR(128)    NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedBy DEFAULT SYSTEM_USER,

        -- Drift detection (optional)
        Checksum        VARBINARY(32)    NULL,

        -- Outcome
        Success         BIT              NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_Success DEFAULT (1),
        Notes           NVARCHAR(MAX)    NULL,

        CONSTRAINT PK_System_SchemaMigrations PRIMARY KEY CLUSTERED (MigrationId),
        CONSTRAINT UQ_System_SchemaMigrations_ScriptName UNIQUE (ScriptName)
    );
END
GO
