-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditEngagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           AuditEngagement.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the AuditEngagement aggregate root backing
--                  IAuditEngagementRepository. Lives in the [Auditing] schema -- deliberately
--                  distinct from the pre-existing [Compliance] schema used by
--                  Infrastructure.Compliance.Auditing (application/software audit read-models,
--                  e.g. [Compliance].[usp_AuditFindings_GetFindings]) so the two modules never
--                  collide on schema, table, or procedure names. TenantId is NOT NULL so this
--                  table is automatically picked up by the live-discovery Row-Level-Security
--                  policy in 90-row-level-security.sql. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Auditing')
BEGIN
    EXEC('CREATE SCHEMA [Auditing]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditEngagement' AND schema_id = SCHEMA_ID('Auditing'))
BEGIN
    CREATE TABLE [Auditing].[AuditEngagement] (
        [AuditEngagementId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER NULL,
        [Status]            NVARCHAR(20)     NOT NULL CONSTRAINT DF_AuditEngagement_Status DEFAULT ('Created'),
        [EngagementTitle]   NVARCHAR(200)    NOT NULL,
        [DisciplineType]    NVARCHAR(50)     NOT NULL,
        [Scope]             NVARCHAR(2000)   NOT NULL CONSTRAINT DF_AuditEngagement_Scope DEFAULT (''),
        [LeadAuditorId]     UNIQUEIDENTIFIER NULL,
        [ScheduledAtUtc]    DATETIME2(3)     NULL,
        [CompletedAtUtc]    DATETIME2(3)     NULL,
        [FindingsCount]     INT              NOT NULL CONSTRAINT DF_AuditEngagement_FindingsCount DEFAULT (0),
        [ComplianceScore]   DECIMAL(5,2)     NULL,
        [ComplianceRating]  NVARCHAR(30)     NOT NULL CONSTRAINT DF_AuditEngagement_ComplianceRating DEFAULT ('NotRated'),
        [CreatedAtUtc]      DATETIME2(3)     NOT NULL,
        [CreatedBy]         UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted]         BIT              NOT NULL CONSTRAINT DF_AuditEngagement_IsDeleted DEFAULT (0),
        [LastModifiedAtUtc] DATETIME2(3)     NOT NULL CONSTRAINT DF_AuditEngagement_LastModified DEFAULT (SYSUTCDATETIME()),
        [RowVersion]        ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AuditEngagement_TenantId_SiteId]
        ON [Auditing].[AuditEngagement] ([TenantId], [SiteId])
        INCLUDE ([Status], [DisciplineType], [EngagementTitle]);
END
GO
