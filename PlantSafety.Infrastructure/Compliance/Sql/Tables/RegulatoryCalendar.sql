-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RegulatoryCalendar
--  Layer:          Infrastructure -- SQL Scripts
--  File:           RegulatoryCalendar.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the RegulatoryCalendar aggregate root
--                  (IRegulatoryCalendarRepository). Tenant-scoped (TenantId NOT NULL
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RegulatoryCalendar' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[RegulatoryCalendar]
    (
        RegulatoryCalendarId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RegulatoryCalendar PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        SiteId                UNIQUEIDENTIFIER NULL,
        Status                NVARCHAR(50)     NOT NULL,
        EventTitle            NVARCHAR(400)    NOT NULL,
        RegulatoryBody        NVARCHAR(400)    NOT NULL,
        JurisdictionCode      NVARCHAR(50)     NOT NULL,
        ComplianceCategory    NVARCHAR(200)    NOT NULL,
        ResponsiblePartyId    UNIQUEIDENTIFIER NOT NULL,
        DueAtUtc              DATETIME2(3)     NULL,
        ReminderLeadDays      INT              NOT NULL CONSTRAINT DF_RegulatoryCalendar_ReminderLeadDays DEFAULT (30),
        CreatedAtUtc          DATETIME2(3)     NOT NULL,
        CreatedBy             UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc          DATETIME2(3)     NULL,
        UpdatedBy             UNIQUEIDENTIFIER NULL,
        IsDeleted             BIT              NOT NULL CONSTRAINT DF_RegulatoryCalendar_IsDeleted DEFAULT (0),
        RowVersion            ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_RegulatoryCalendar_TenantId_Site ON [Compliance].[RegulatoryCalendar] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_RegulatoryCalendar_Status ON [Compliance].[RegulatoryCalendar] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_RegulatoryCalendar_CreatedAtUtc ON [Compliance].[RegulatoryCalendar] (CreatedAtUtc);
    CREATE NONCLUSTERED INDEX IX_RegulatoryCalendar_DueAtUtc ON [Compliance].[RegulatoryCalendar] (DueAtUtc);
END
GO
