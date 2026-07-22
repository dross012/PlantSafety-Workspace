-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.ActiveMonitoringAuditing
--  Layer:          Infrastructure - SQL Scripts
--  File:           ActiveMonitoringAuditing.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ActiveMonitoringAuditing aggregate root backing IActiveMonitoringAuditingRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ContractorSafety')
BEGIN
    EXEC('CREATE SCHEMA [ContractorSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ActiveMonitoringAuditing' AND schema_id = SCHEMA_ID('ContractorSafety'))
BEGIN
    CREATE TABLE [ContractorSafety].[ActiveMonitoringAuditing] (
        [ActiveMonitoringAuditingId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [MonitoringTitle] NVARCHAR(400) NOT NULL,
        [Category] NVARCHAR(100) NOT NULL,
        [WorkLocation] NVARCHAR(400) NOT NULL,
        [MonitoredByUserId] UNIQUEIDENTIFIER NOT NULL,
        [ObservedAtUtc] DATETIME2(3) NULL,
        [UnsafeConditionCount] INT NOT NULL CONSTRAINT DF_ActiveMonitoringAuditing_UnsafeConditionCount DEFAULT (0),
        [ComplianceScore] DECIMAL(9,2) NULL,
        [FindingNotes] NVARCHAR(2000) NULL,
        [FollowUpAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_ActiveMonitoringAuditing_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ActiveMonitoringAuditing_TenantId_SiteId]
        ON [ContractorSafety].[ActiveMonitoringAuditing] ([TenantId], [SiteId])
        INCLUDE ([Status], [MonitoringTitle]);
END
GO
