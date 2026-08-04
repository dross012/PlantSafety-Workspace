-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CommunitySafety.CommunityConcern
--  Layer:          Infrastructure - SQL Scripts
--  File:           CommunityConcern.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the CommunityConcern aggregate root (primary aggregate for the
--                  CommunitySafety module) backing ICommunityConcernRepository. TenantId is NOT
--                  NULL so this table is automatically picked up by the live-discovery
--                  Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate). Retention: resolved concern records are
--                  retained per the public-records/regulatory retention requirement (see
--                  CommunityConcernArchivalPolicy) before archival. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'CommunitySafety')
BEGIN
    EXEC('CREATE SCHEMA [CommunitySafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CommunityConcern' AND schema_id = SCHEMA_ID('CommunitySafety'))
BEGIN
    CREATE TABLE [CommunitySafety].[CommunityConcern] (
        [CommunityConcernId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NOT NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [Category] NVARCHAR(50) NOT NULL,
        [Severity] NVARCHAR(30) NOT NULL,
        [FacilityArea] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(4000) NOT NULL,
        [ReportedByContactId] UNIQUEIDENTIFIER NULL,
        [RelatedIncidentId] UNIQUEIDENTIFIER NULL,
        [LoggedAtUtc] DATETIME2(3) NOT NULL,
        [EscalationCount] INT NOT NULL CONSTRAINT DF_CommunityConcern_EscalationCount DEFAULT (0),
        [EscalationReason] NVARCHAR(1000) NULL,
        [EscalatedAtUtc] DATETIME2(3) NULL,
        [ResolutionNotes] NVARCHAR(4000) NULL,
        [ResolvedAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_CommunityConcern_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CommunityConcern_TenantId_SiteId]
        ON [CommunitySafety].[CommunityConcern] ([TenantId], [SiteId])
        INCLUDE ([Status], [Category], [Severity], [FacilityArea]);

    CREATE NONCLUSTERED INDEX [IX_CommunityConcern_FacilityArea_LoggedAtUtc]
        ON [CommunitySafety].[CommunityConcern] ([TenantId], [FacilityArea], [LoggedAtUtc])
        INCLUDE ([Status]);
END
GO
