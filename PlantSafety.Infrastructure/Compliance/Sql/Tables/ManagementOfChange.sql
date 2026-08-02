-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange
--  Layer:          Infrastructure -- SQL Scripts
--  File:           ManagementOfChange.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the ManagementOfChange aggregate root
--                  (IManagementOfChangeRepository). Tenant-scoped (TenantId NOT NULL
--                  uniqueidentifier). Phase 4 full build-out: this aggregate previously had zero
--                  persistence at all. Mirrors [Compliance].[Permits]'s native ROWVERSION
--                  concurrency convention from day one (no retrofit needed).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ManagementOfChange' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ManagementOfChange]
    (
        ManagementOfChangeId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ManagementOfChange PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        SiteId                UNIQUEIDENTIFIER NULL,
        Status                NVARCHAR(50)     NOT NULL,
        ChangeTitle           NVARCHAR(400)    NOT NULL,
        ChangeType            NVARCHAR(100)    NOT NULL,
        Justification         NVARCHAR(2000)   NOT NULL,
        RiskLevel             NVARCHAR(50)     NOT NULL,
        RequestedByUserId     UNIQUEIDENTIFIER NOT NULL,
        ApprovedByUserId      UNIQUEIDENTIFIER NULL,
        ApprovalStatus        NVARCHAR(50)     NOT NULL,
        ImplementationAtUtc   DATETIME2(3)     NULL,
        ReviewDueAtUtc        DATETIME2(3)     NULL,
        CreatedAtUtc          DATETIME2(3)     NOT NULL,
        CreatedBy             UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc          DATETIME2(3)     NULL,
        UpdatedBy             UNIQUEIDENTIFIER NULL,
        IsDeleted             BIT              NOT NULL CONSTRAINT DF_ManagementOfChange_IsDeleted DEFAULT (0),
        RowVersion            ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_ManagementOfChange_TenantId_Site ON [Compliance].[ManagementOfChange] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_ManagementOfChange_Status ON [Compliance].[ManagementOfChange] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_ManagementOfChange_CreatedAtUtc ON [Compliance].[ManagementOfChange] (CreatedAtUtc);
END
GO
