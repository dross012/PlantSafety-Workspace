-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyCulture.CultureAssessment
--  Layer:          Infrastructure - SQL Scripts
--  File:           CultureAssessment.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the CultureAssessment aggregate root backing
--                  ICultureAssessmentRepository. Lives in the [SafetyCulture] schema --
--                  deliberately distinct from the pre-existing [Analytics] schema used by
--                  Infrastructure.Analytics.Culture (a different module: cross-module safety
--                  culture index rollups) so the two never collide on schema, table, or
--                  procedure names. TenantId is NOT NULL so this table is automatically
--                  picked up by the live-discovery Row-Level-Security policy in
--                  90-row-level-security.sql. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'SafetyCulture')
BEGIN
    EXEC('CREATE SCHEMA [SafetyCulture]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CultureAssessment' AND schema_id = SCHEMA_ID('SafetyCulture'))
BEGIN
    CREATE TABLE [SafetyCulture].[CultureAssessment] (
        [CultureAssessmentId]           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]                      UNIQUEIDENTIFIER NOT NULL,
        [SiteId]                        UNIQUEIDENTIFIER NULL,
        [Status]                        NVARCHAR(20)     NOT NULL CONSTRAINT DF_CultureAssessment_Status DEFAULT ('Created'),
        [AssessmentTitle]               NVARCHAR(200)    NOT NULL,
        [PeriodStartUtc]                DATETIME2(3)     NOT NULL,
        [PeriodEndUtc]                  DATETIME2(3)     NOT NULL,
        [OverallCultureScore]           DECIMAL(5,2)     NULL,
        [LeadershipEngagementScore]     DECIMAL(5,2)     NULL,
        [Summary]                       NVARCHAR(2000)   NOT NULL CONSTRAINT DF_CultureAssessment_Summary DEFAULT (''),
        [RelatedRiskManagementHazardId] UNIQUEIDENTIFIER NULL,
        [CreatedAtUtc]                  DATETIME2(3)     NOT NULL,
        [CreatedBy]                     UNIQUEIDENTIFIER NOT NULL,
        [PublishedAtUtc]                DATETIME2(3)     NULL,
        [IsDeleted]                     BIT              NOT NULL CONSTRAINT DF_CultureAssessment_IsDeleted DEFAULT (0),
        [LastModifiedAtUtc]             DATETIME2(3)     NOT NULL CONSTRAINT DF_CultureAssessment_LastModified DEFAULT (SYSUTCDATETIME()),
        [RowVersion]                    ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CultureAssessment_TenantId_SiteId]
        ON [SafetyCulture].[CultureAssessment] ([TenantId], [SiteId])
        INCLUDE ([Status], [PeriodStartUtc], [PeriodEndUtc]);
END
GO
