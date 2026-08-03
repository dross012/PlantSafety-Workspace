-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPrograms.SafetyProgram
--  Layer:          Infrastructure - SQL Scripts
--  File:           SafetyProgram.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the SafetyProgram aggregate root backing
--                  ISafetyProgramRepository. Goals are flattened to a single delimited
--                  NVARCHAR(MAX) column (see SafetyProgramDapperRepository remarks).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'SafetyPrograms')
BEGIN
    EXEC('CREATE SCHEMA [SafetyPrograms]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SafetyProgram' AND schema_id = SCHEMA_ID('SafetyPrograms'))
BEGIN
    CREATE TABLE [SafetyPrograms].[SafetyProgram] (
        [SafetyProgramId]     UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]            UNIQUEIDENTIFIER NOT NULL,
        [SiteId]              UNIQUEIDENTIFIER NULL,
        [Status]              NVARCHAR(20)     NOT NULL CONSTRAINT DF_SafetyProgram_Status DEFAULT ('Planned'),
        [Name]                NVARCHAR(256)    NOT NULL,
        [Description]         NVARCHAR(4000)   NOT NULL,
        [Category]            NVARCHAR(40)     NOT NULL,
        [StartDate]           DATE             NOT NULL,
        [EndDate]             DATE             NULL,
        [OwnerId]             UNIQUEIDENTIFIER NOT NULL,
        [ParticipationPercent] FLOAT           NOT NULL CONSTRAINT DF_SafetyProgram_ParticipationPercent DEFAULT (0),
        [EnrolledCount]       INT              NOT NULL CONSTRAINT DF_SafetyProgram_EnrolledCount DEFAULT (0),
        [Goals]               NVARCHAR(MAX)    NULL,
        [CreatedAtUtc]        DATETIME2(3)     NOT NULL,
        [CreatedBy]           UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted]           BIT              NOT NULL CONSTRAINT DF_SafetyProgram_IsDeleted DEFAULT (0),
        [LastModifiedAtUtc]   DATETIME2(3)     NOT NULL CONSTRAINT DF_SafetyProgram_LastModified DEFAULT (SYSUTCDATETIME()),
        [RowVersion]          ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_SafetyProgram_TenantId_SiteId_Status]
        ON [SafetyPrograms].[SafetyProgram] ([TenantId], [SiteId], [Status])
        INCLUDE ([Name], [Category], [ParticipationPercent], [OwnerId]);
END
GO
