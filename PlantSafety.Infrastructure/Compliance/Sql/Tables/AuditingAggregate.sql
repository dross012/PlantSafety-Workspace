-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Auditing
--  Layer:          Infrastructure -- SQL Scripts
--  File:           AuditingAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the Auditing aggregate root (IAuditingRepository). Tenant-scoped
--                  (TenantId NOT NULL uniqueidentifier). Named "AuditingAggregate" (not bare
--                  "Auditing") to avoid any ambiguity with the pre-existing read-only category
--                  slices (AuditFindings/CAPATriggers/ComplianceAudit/InternalSafetyAudit) that
--                  live under the same Compliance.Auditing module folder. Phase 4 full build-out:
--                  this aggregate previously had zero persistence at all. Mirrors
--                  [Compliance].[Permits]'s native ROWVERSION concurrency convention from day one.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditingAggregate' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[AuditingAggregate]
    (
        AuditingId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AuditingAggregate PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        Status                 NVARCHAR(50)     NOT NULL,
        AuditTitle             NVARCHAR(400)    NOT NULL,
        AuditType              NVARCHAR(100)    NOT NULL,
        Scope                  NVARCHAR(2000)   NOT NULL,
        AuditorId              UNIQUEIDENTIFIER NOT NULL,
        ScheduledAtUtc         DATETIME2(3)     NULL,
        CompletedAtUtc         DATETIME2(3)     NULL,
        FindingsCount          INT              NOT NULL CONSTRAINT DF_AuditingAggregate_FindingsCount DEFAULT (0),
        Score                  DECIMAL(5,2)     NULL,
        ComplianceRating       NVARCHAR(50)     NOT NULL CONSTRAINT DF_AuditingAggregate_ComplianceRating DEFAULT (N'NotRated'),
        CreatedAtUtc           DATETIME2(3)     NOT NULL,
        CreatedBy              UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc           DATETIME2(3)     NULL,
        UpdatedBy              UNIQUEIDENTIFIER NULL,
        IsDeleted              BIT              NOT NULL CONSTRAINT DF_AuditingAggregate_IsDeleted DEFAULT (0),
        RowVersion             ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_AuditingAggregate_TenantId_Site ON [Compliance].[AuditingAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_AuditingAggregate_Status ON [Compliance].[AuditingAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_AuditingAggregate_CreatedAtUtc ON [Compliance].[AuditingAggregate] (CreatedAtUtc);
END
GO
