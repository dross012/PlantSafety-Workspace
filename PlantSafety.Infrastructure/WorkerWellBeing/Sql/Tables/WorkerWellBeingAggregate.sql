-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         WorkerWellBeing
--  Layer:          Infrastructure -- SQL Scripts
--  File:           WorkerWellBeingAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the WorkerWellBeing aggregate root (IWorkerWellBeingRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier). Named
--                  "WorkerWellBeingAggregate" (not bare "WorkerWellBeing") to avoid any ambiguity
--                  with the pre-existing, unrelated Features-vertical
--                  [WorkerWellBeing].[WorkerWellBeingRecords] table that lives under this same
--                  module folder (backs the generic wellbeing-record tracking Features slice, a
--                  different concept). Phase 4 full build-out: this aggregate previously had zero
--                  persistence at all. Mirrors [Compliance].[AuditingAggregate]'s native
--                  ROWVERSION concurrency convention from day one. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'WorkerWellBeing')
    EXEC('CREATE SCHEMA WorkerWellBeing AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkerWellBeingAggregate' AND schema_id = SCHEMA_ID('WorkerWellBeing'))
BEGIN
    CREATE TABLE [WorkerWellBeing].[WorkerWellBeingAggregate]
    (
        WorkerWellBeingId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkerWellBeingAggregate PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        WorkerId                UNIQUEIDENTIFIER NOT NULL,
        WellBeingRiskLevel      NVARCHAR(50)     NOT NULL CONSTRAINT DF_WorkerWellBeingAggregate_RiskLevel DEFAULT (N'Low'),
        CheckInType             NVARCHAR(100)    NOT NULL,
        WellBeingScore          INT              NOT NULL,
        FollowUpRequired        BIT              NOT NULL CONSTRAINT DF_WorkerWellBeingAggregate_FollowUp DEFAULT (0),
        Status                  NVARCHAR(50)     NOT NULL CONSTRAINT DF_WorkerWellBeingAggregate_Status DEFAULT (N'Created'),
        RecordedAtUtc           DATETIME2(3)     NOT NULL,
        CreatedAtUtc            DATETIME2(3)     NOT NULL,
        CreatedBy               UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc            DATETIME2(3)     NULL,
        UpdatedBy               UNIQUEIDENTIFIER NULL,
        IsDeleted               BIT              NOT NULL CONSTRAINT DF_WorkerWellBeingAggregate_IsDeleted DEFAULT (0),
        RowVersion              ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_WorkerWellBeingAggregate_TenantId_Site ON [WorkerWellBeing].[WorkerWellBeingAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_WorkerWellBeingAggregate_Status ON [WorkerWellBeing].[WorkerWellBeingAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_WorkerWellBeingAggregate_CreatedAtUtc ON [WorkerWellBeing].[WorkerWellBeingAggregate] (CreatedAtUtc);
END
GO
