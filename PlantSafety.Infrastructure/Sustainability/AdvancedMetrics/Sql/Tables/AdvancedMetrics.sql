-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Sustainability.AdvancedMetrics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           AdvancedMetrics.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Tables backing the Sustainability.AdvancedMetrics sub-module: the
--                  AdvancedMetricsLedger aggregate header plus its five child record tables
--                  (CarbonEmissionRecord, EnergyIntensityMetric, WaterIntensityMetric,
--                  WasteDiversionRecord, SustainabilityTarget) and the separate
--                  EsgDisclosureReport aggregate. New schema [SustainabilityAdvancedMetrics] --
--                  deliberately distinct from the pre-existing [Sustainability] schema to avoid
--                  any table/procedure name collision with the generic Sustainability aggregate
--                  and the legacy Features-vertical [Sustainability].[Metric] table, per the
--                  Phase 5 "choose the right schema" convention. Every table carries a NOT NULL
--                  TenantId for Row-Level-Security discovery. Every child record table carries its
--                  own MethodologyVersion column so historical reports built from it stay
--                  reproducible even if the calculation methodology changes later (data-governance
--                  requirement). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'SustainabilityAdvancedMetrics')
    EXEC('CREATE SCHEMA SustainabilityAdvancedMetrics AUTHORIZATION dbo;');
GO

-- ---- AdvancedMetricsLedger (aggregate header) --------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AdvancedMetricsLedger' AND schema_id = SCHEMA_ID('SustainabilityAdvancedMetrics'))
BEGIN
    CREATE TABLE [SustainabilityAdvancedMetrics].[AdvancedMetricsLedger]
    (
        AdvancedMetricsLedgerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AdvancedMetricsLedger PRIMARY KEY,
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        SiteId                   UNIQUEIDENTIFIER NULL,
        ReportingPeriodStartUtc  DATETIME2(3)     NOT NULL,
        ReportingPeriodEndUtc    DATETIME2(3)     NOT NULL,
        Status                   NVARCHAR(50)     NOT NULL CONSTRAINT DF_AdvancedMetricsLedger_Status DEFAULT (N'Open'),
        CreatedAtUtc             DATETIME2(3)     NOT NULL,
        CreatedBy                UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc             DATETIME2(3)     NULL,
        UpdatedBy                UNIQUEIDENTIFIER NULL,
        IsDeleted                BIT              NOT NULL CONSTRAINT DF_AdvancedMetricsLedger_IsDeleted DEFAULT (0),
        RowVersion               ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_AdvancedMetricsLedger_TenantId_Site ON [SustainabilityAdvancedMetrics].[AdvancedMetricsLedger] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_AdvancedMetricsLedger_OpenPeriod ON [SustainabilityAdvancedMetrics].[AdvancedMetricsLedger] (TenantId, SiteId, ReportingPeriodStartUtc, ReportingPeriodEndUtc, Status) WHERE IsDeleted = 0;
END
GO

-- ---- CarbonEmissionRecord (child) --------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CarbonEmissionRecord' AND schema_id = SCHEMA_ID('SustainabilityAdvancedMetrics'))
BEGIN
    CREATE TABLE [SustainabilityAdvancedMetrics].[CarbonEmissionRecord]
    (
        CarbonEmissionRecordId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CarbonEmissionRecord PRIMARY KEY,
        AdvancedMetricsLedgerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CarbonEmissionRecord_Ledger REFERENCES [SustainabilityAdvancedMetrics].[AdvancedMetricsLedger] (AdvancedMetricsLedgerId),
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        Scope                    NVARCHAR(20)     NOT NULL,
        ActivityDescription      NVARCHAR(400)    NOT NULL,
        EmissionsMetricTonsCo2e  DECIMAL(18,6)    NOT NULL,
        MethodologyVersion       NVARCHAR(100)    NOT NULL,
        ActivityAtUtc            DATETIME2(3)     NOT NULL,
        RecordedAtUtc            DATETIME2(3)     NOT NULL,
        RecordedBy               UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_CarbonEmissionRecord_Ledger ON [SustainabilityAdvancedMetrics].[CarbonEmissionRecord] (AdvancedMetricsLedgerId);
    CREATE NONCLUSTERED INDEX IX_CarbonEmissionRecord_TenantId_Scope ON [SustainabilityAdvancedMetrics].[CarbonEmissionRecord] (TenantId, Scope);
END
GO

-- ---- EnergyIntensityMetric (child) -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EnergyIntensityMetric' AND schema_id = SCHEMA_ID('SustainabilityAdvancedMetrics'))
BEGIN
    CREATE TABLE [SustainabilityAdvancedMetrics].[EnergyIntensityMetric]
    (
        EnergyIntensityMetricId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EnergyIntensityMetric PRIMARY KEY,
        AdvancedMetricsLedgerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_EnergyIntensityMetric_Ledger REFERENCES [SustainabilityAdvancedMetrics].[AdvancedMetricsLedger] (AdvancedMetricsLedgerId),
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        EnergyConsumedKwh        DECIMAL(18,6)    NOT NULL,
        OutputUnits              DECIMAL(18,6)    NOT NULL,
        IntensityValue           DECIMAL(18,6)    NOT NULL,
        MethodologyVersion       NVARCHAR(100)    NOT NULL,
        ReadingAtUtc             DATETIME2(3)     NOT NULL,
        RecordedAtUtc            DATETIME2(3)     NOT NULL,
        RecordedBy               UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_EnergyIntensityMetric_Ledger ON [SustainabilityAdvancedMetrics].[EnergyIntensityMetric] (AdvancedMetricsLedgerId, ReadingAtUtc);
END
GO

-- ---- WaterIntensityMetric (child) --------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WaterIntensityMetric' AND schema_id = SCHEMA_ID('SustainabilityAdvancedMetrics'))
BEGIN
    CREATE TABLE [SustainabilityAdvancedMetrics].[WaterIntensityMetric]
    (
        WaterIntensityMetricId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WaterIntensityMetric PRIMARY KEY,
        AdvancedMetricsLedgerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_WaterIntensityMetric_Ledger REFERENCES [SustainabilityAdvancedMetrics].[AdvancedMetricsLedger] (AdvancedMetricsLedgerId),
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        WaterConsumedCubicMeters DECIMAL(18,6)    NOT NULL,
        OutputUnits              DECIMAL(18,6)    NOT NULL,
        IntensityValue           DECIMAL(18,6)    NOT NULL,
        MethodologyVersion       NVARCHAR(100)    NOT NULL,
        ReadingAtUtc             DATETIME2(3)     NOT NULL,
        RecordedAtUtc            DATETIME2(3)     NOT NULL,
        RecordedBy               UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_WaterIntensityMetric_Ledger ON [SustainabilityAdvancedMetrics].[WaterIntensityMetric] (AdvancedMetricsLedgerId, ReadingAtUtc);
END
GO

-- ---- WasteDiversionRecord (child) --------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WasteDiversionRecord' AND schema_id = SCHEMA_ID('SustainabilityAdvancedMetrics'))
BEGIN
    CREATE TABLE [SustainabilityAdvancedMetrics].[WasteDiversionRecord]
    (
        WasteDiversionRecordId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WasteDiversionRecord PRIMARY KEY,
        AdvancedMetricsLedgerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_WasteDiversionRecord_Ledger REFERENCES [SustainabilityAdvancedMetrics].[AdvancedMetricsLedger] (AdvancedMetricsLedgerId),
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        TotalWasteKg             DECIMAL(18,4)    NOT NULL,
        DivertedWasteKg          DECIMAL(18,4)    NOT NULL,
        DiversionRatePercent     DECIMAL(5,2)     NOT NULL,
        MethodologyVersion       NVARCHAR(100)    NOT NULL,
        IntervalEndAtUtc         DATETIME2(3)     NOT NULL,
        RecordedAtUtc            DATETIME2(3)     NOT NULL,
        RecordedBy               UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_WasteDiversionRecord_Ledger ON [SustainabilityAdvancedMetrics].[WasteDiversionRecord] (AdvancedMetricsLedgerId, IntervalEndAtUtc);
END
GO

-- ---- SustainabilityTarget (child, mutable) -------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SustainabilityTarget' AND schema_id = SCHEMA_ID('SustainabilityAdvancedMetrics'))
BEGIN
    CREATE TABLE [SustainabilityAdvancedMetrics].[SustainabilityTarget]
    (
        SustainabilityTargetId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SustainabilityTarget PRIMARY KEY,
        AdvancedMetricsLedgerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_SustainabilityTarget_Ledger REFERENCES [SustainabilityAdvancedMetrics].[AdvancedMetricsLedger] (AdvancedMetricsLedgerId),
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        TargetType               NVARCHAR(50)     NOT NULL,
        BaselineValue            DECIMAL(18,6)    NOT NULL,
        TargetValue              DECIMAL(18,6)    NOT NULL,
        CurrentValue             DECIMAL(18,6)    NOT NULL,
        TargetDateUtc            DATETIME2(3)     NOT NULL,
        Status                   NVARCHAR(20)     NOT NULL CONSTRAINT DF_SustainabilityTarget_Status DEFAULT (N'OnTrack'),
        SetAtUtc                 DATETIME2(3)     NOT NULL,
        SetBy                    UNIQUEIDENTIFIER NOT NULL,
        LastProgressUpdateAtUtc  DATETIME2(3)     NULL
    );

    CREATE NONCLUSTERED INDEX IX_SustainabilityTarget_Ledger ON [SustainabilityAdvancedMetrics].[SustainabilityTarget] (AdvancedMetricsLedgerId);
    CREATE NONCLUSTERED INDEX IX_SustainabilityTarget_Status ON [SustainabilityAdvancedMetrics].[SustainabilityTarget] (TenantId, Status);
END
GO

-- ---- EsgDisclosureReport (separate aggregate) ----------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EsgDisclosureReport' AND schema_id = SCHEMA_ID('SustainabilityAdvancedMetrics'))
BEGIN
    CREATE TABLE [SustainabilityAdvancedMetrics].[EsgDisclosureReport]
    (
        EsgDisclosureReportId   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EsgDisclosureReport PRIMARY KEY,
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        SiteId                   UNIQUEIDENTIFIER NULL,
        SourceLedgerId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_EsgDisclosureReport_Ledger REFERENCES [SustainabilityAdvancedMetrics].[AdvancedMetricsLedger] (AdvancedMetricsLedgerId),
        Frameworks               NVARCHAR(100)    NOT NULL,
        ReportingPeriodStartUtc  DATETIME2(3)     NOT NULL,
        ReportingPeriodEndUtc    DATETIME2(3)     NOT NULL,
        Version                  INT              NOT NULL CONSTRAINT DF_EsgDisclosureReport_Version DEFAULT (1),
        SupersedesReportId       UNIQUEIDENTIFIER NULL,
        MethodologyVersion       NVARCHAR(100)    NOT NULL,
        Status                   NVARCHAR(20)     NOT NULL CONSTRAINT DF_EsgDisclosureReport_Status DEFAULT (N'Draft'),
        GeneratedAtUtc           DATETIME2(3)     NOT NULL,
        GeneratedBy              UNIQUEIDENTIFIER NOT NULL,
        PublishedAtUtc           DATETIME2(3)     NULL,
        IsDeleted                BIT              NOT NULL CONSTRAINT DF_EsgDisclosureReport_IsDeleted DEFAULT (0)
    );

    CREATE NONCLUSTERED INDEX IX_EsgDisclosureReport_TenantId_Site ON [SustainabilityAdvancedMetrics].[EsgDisclosureReport] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_EsgDisclosureReport_SourceLedger ON [SustainabilityAdvancedMetrics].[EsgDisclosureReport] (SourceLedgerId);
    CREATE NONCLUSTERED INDEX IX_EsgDisclosureReport_Supersedes ON [SustainabilityAdvancedMetrics].[EsgDisclosureReport] (SupersedesReportId);
END
GO
