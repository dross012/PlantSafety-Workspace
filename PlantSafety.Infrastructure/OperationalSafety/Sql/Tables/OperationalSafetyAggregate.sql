-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         OperationalSafety
--  Layer:          Infrastructure -- SQL Scripts
--  File:           OperationalSafetyAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the OperationalSafety aggregate root
--                  (IOperationalSafetyRepository). Tenant-scoped (TenantId NOT NULL
--                  uniqueidentifier). Named "OperationalSafetyAggregate" (not bare
--                  "OperationalSafety") for clarity and to avoid any ambiguity with the
--                  pre-existing, dead OperationalSafetyStub.cs scaffolding and the pre-existing,
--                  unrelated, LIVE OperationalSafety.Alerts/Checklists/PPEManagement/
--                  RiskManagement/Schedules tables that live under this same module folder.
--                  Phase 4 full build-out: this aggregate previously had zero persistence at
--                  all. Mirrors [Compliance].[AuditingAggregate]'s native ROWVERSION concurrency
--                  convention from day one. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'OperationalSafety')
    EXEC('CREATE SCHEMA OperationalSafety AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OperationalSafetyAggregate' AND schema_id = SCHEMA_ID('OperationalSafety'))
BEGIN
    CREATE TABLE [OperationalSafety].[OperationalSafetyAggregate]
    (
        OperationalSafetyId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OperationalSafetyAggregate PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        SiteId                UNIQUEIDENTIFIER NULL,
        Status                NVARCHAR(50)     NOT NULL,
        SafetyDomain          NVARCHAR(200)    NOT NULL,
        RiskLevel             NVARCHAR(50)     NOT NULL CONSTRAINT DF_OperationalSafetyAggregate_RiskLevel DEFAULT (N'Low'),
        OwnerUserId           UNIQUEIDENTIFIER NOT NULL,
        ReviewDueAtUtc        DATETIME2(3)     NULL,
        CreatedAtUtc          DATETIME2(3)     NOT NULL,
        CreatedBy             UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc          DATETIME2(3)     NULL,
        UpdatedBy             UNIQUEIDENTIFIER NULL,
        IsDeleted             BIT              NOT NULL CONSTRAINT DF_OperationalSafetyAggregate_IsDeleted DEFAULT (0),
        RowVersion            ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_OperationalSafetyAggregate_TenantId_Site ON [OperationalSafety].[OperationalSafetyAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_OperationalSafetyAggregate_Status ON [OperationalSafety].[OperationalSafetyAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_OperationalSafetyAggregate_CreatedAtUtc ON [OperationalSafety].[OperationalSafetyAggregate] (CreatedAtUtc);
END
GO
