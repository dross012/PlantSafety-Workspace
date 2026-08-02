-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits
--  Layer:          Infrastructure -- SQL Scripts
--  File:           Permits.sql
--  Created:        2026-07-30
--  SchemaVersion:  2026.1
--  Description:    Table backing the Permits aggregate root (IPermitsRepository). Tenant-scoped
--                  (TenantId NOT NULL uniqueidentifier). Distinct from the pre-existing
--                  [Compliance].[Permit] (singular) table owned by the separate
--                  PlantSafety.Application.Features.Compliance "Features" tree -- that table is
--                  untouched by this migration. Phase 4 full build-out: this aggregate previously
--                  had zero persistence at all. Mirrors [PermitToWork].[PermitWorkflow]'s native
--                  ROWVERSION concurrency convention from day one (no retrofit needed).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Permits' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[Permits]
    (
        PermitsId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Permits PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        SiteId                UNIQUEIDENTIFIER NULL,
        Status                NVARCHAR(50)     NOT NULL,
        PermitType            NVARCHAR(400)    NOT NULL,
        PermitNumber          NVARCHAR(400)    NOT NULL,
        WorkDescription       NVARCHAR(2000)   NOT NULL,
        Location              NVARCHAR(400)    NOT NULL,
        HazardsIdentified     NVARCHAR(2000)   NOT NULL,
        RequestedByUserId     UNIQUEIDENTIFIER NOT NULL,
        ApprovedByUserId      UNIQUEIDENTIFIER NULL,
        IssuedAtUtc           DATETIME2(3)     NULL,
        ExpiresAtUtc          DATETIME2(3)     NULL,
        RejectedReason        NVARCHAR(2000)   NULL,
        RejectedAtUtc         DATETIME2(3)     NULL,
        CompletedAtUtc        DATETIME2(3)     NULL,
        ExpiredAtUtc          DATETIME2(3)     NULL,
        ClosedAtUtc           DATETIME2(3)     NULL,
        ArchivedAtUtc         DATETIME2(3)     NULL,
        CreatedAtUtc          DATETIME2(3)     NOT NULL,
        CreatedBy             UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc          DATETIME2(3)     NULL,
        UpdatedBy             UNIQUEIDENTIFIER NULL,
        IsDeleted             BIT              NOT NULL CONSTRAINT DF_Permits_IsDeleted DEFAULT (0),
        RowVersion            ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Permits_TenantId_Site ON [Compliance].[Permits] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_Permits_Status ON [Compliance].[Permits] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_Permits_CreatedAtUtc ON [Compliance].[Permits] (CreatedAtUtc);
END
GO
