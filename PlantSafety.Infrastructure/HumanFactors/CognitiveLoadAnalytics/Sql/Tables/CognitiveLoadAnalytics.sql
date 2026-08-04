-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors.CognitiveLoadAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           CognitiveLoadAnalytics.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Tables backing the HumanFactors.CognitiveLoadAnalytics sub-module: the
--                  CognitiveLoadLedger aggregate header plus its three child record tables
--                  (CognitiveLoadReading, TaskComplexityProfile, WorkloadTrendRecord) and the
--                  separate CognitiveRiskPrediction aggregate. New schema
--                  [HumanFactorsCognitiveLoadAnalytics] -- deliberately distinct from the
--                  pre-existing [HumanFactors] schema to avoid any table/procedure name collision
--                  with the generic HumanFactors aggregate, mirroring the
--                  [SustainabilityAdvancedMetrics] precedent. Every table carries a NOT NULL
--                  TenantId for Row-Level-Security discovery. Data governance: CognitiveLoadReading
--                  carries individually identifiable worker cognitive-load data -- sensitive
--                  personal data with restricted access and a retention window tied to the HR
--                  data policy (active employment + defined window), enforced via the ledger's
--                  Archive lifecycle step. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HumanFactorsCognitiveLoadAnalytics')
    EXEC('CREATE SCHEMA HumanFactorsCognitiveLoadAnalytics AUTHORIZATION dbo;');
GO

-- ---- CognitiveLoadLedger (aggregate header) --------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CognitiveLoadLedger' AND schema_id = SCHEMA_ID('HumanFactorsCognitiveLoadAnalytics'))
BEGIN
    CREATE TABLE [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger]
    (
        CognitiveLoadLedgerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CognitiveLoadLedger PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        ShiftType               NVARCHAR(20)     NOT NULL,
        ShiftDateUtc             DATETIME2(3)     NOT NULL,
        Status                   NVARCHAR(50)     NOT NULL CONSTRAINT DF_CognitiveLoadLedger_Status DEFAULT (N'Open'),
        CreatedAtUtc             DATETIME2(3)     NOT NULL,
        CreatedBy                UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc             DATETIME2(3)     NULL,
        UpdatedBy                UNIQUEIDENTIFIER NULL,
        IsDeleted                BIT              NOT NULL CONSTRAINT DF_CognitiveLoadLedger_IsDeleted DEFAULT (0),
        RowVersion               ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_CognitiveLoadLedger_TenantId_Site ON [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_CognitiveLoadLedger_OpenShift ON [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger] (TenantId, SiteId, ShiftType, ShiftDateUtc, Status) WHERE IsDeleted = 0;
END
GO

-- ---- CognitiveLoadReading (child) --------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CognitiveLoadReading' AND schema_id = SCHEMA_ID('HumanFactorsCognitiveLoadAnalytics'))
BEGIN
    CREATE TABLE [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadReading]
    (
        CognitiveLoadReadingId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CognitiveLoadReading PRIMARY KEY,
        CognitiveLoadLedgerId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CognitiveLoadReading_Ledger REFERENCES [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger] (CognitiveLoadLedgerId),
        TenantId                  UNIQUEIDENTIFIER NOT NULL,
        WorkerUserId               UNIQUEIDENTIFIER NOT NULL,
        TaskComplexityProfileId    UNIQUEIDENTIFIER NULL,
        CognitiveLoadScore         DECIMAL(5,2)     NOT NULL,
        WorkloadLevel               NVARCHAR(20)     NOT NULL,
        MeasurementMethod           NVARCHAR(100)    NOT NULL,
        MeasuredAtUtc                DATETIME2(3)     NOT NULL,
        RecordedAtUtc                DATETIME2(3)     NOT NULL,
        RecordedBy                    UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_CognitiveLoadReading_Ledger ON [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadReading] (CognitiveLoadLedgerId, MeasuredAtUtc);
    CREATE NONCLUSTERED INDEX IX_CognitiveLoadReading_TenantId_Worker ON [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadReading] (TenantId, WorkerUserId, MeasuredAtUtc);
END
GO

-- ---- TaskComplexityProfile (child) -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TaskComplexityProfile' AND schema_id = SCHEMA_ID('HumanFactorsCognitiveLoadAnalytics'))
BEGIN
    CREATE TABLE [HumanFactorsCognitiveLoadAnalytics].[TaskComplexityProfile]
    (
        TaskComplexityProfileId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TaskComplexityProfile PRIMARY KEY,
        CognitiveLoadLedgerId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_TaskComplexityProfile_Ledger REFERENCES [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger] (CognitiveLoadLedgerId),
        TenantId                   UNIQUEIDENTIFIER NOT NULL,
        TaskCode                   NVARCHAR(50)     NOT NULL,
        TaskName                   NVARCHAR(200)    NOT NULL,
        ComplexityScore             INT              NOT NULL,
        Description                 NVARCHAR(1000)   NOT NULL CONSTRAINT DF_TaskComplexityProfile_Description DEFAULT (N''),
        DefinedAtUtc                DATETIME2(3)     NOT NULL,
        DefinedBy                    UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_TaskComplexityProfile_Ledger ON [HumanFactorsCognitiveLoadAnalytics].[TaskComplexityProfile] (CognitiveLoadLedgerId);
    CREATE NONCLUSTERED INDEX IX_TaskComplexityProfile_TenantId_Code ON [HumanFactorsCognitiveLoadAnalytics].[TaskComplexityProfile] (TenantId, TaskCode);
END
GO

-- ---- WorkloadTrendRecord (child, append-only) ----------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkloadTrendRecord' AND schema_id = SCHEMA_ID('HumanFactorsCognitiveLoadAnalytics'))
BEGIN
    CREATE TABLE [HumanFactorsCognitiveLoadAnalytics].[WorkloadTrendRecord]
    (
        WorkloadTrendRecordId   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkloadTrendRecord PRIMARY KEY,
        CognitiveLoadLedgerId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_WorkloadTrendRecord_Ledger REFERENCES [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger] (CognitiveLoadLedgerId),
        TenantId                   UNIQUEIDENTIFIER NOT NULL,
        PeriodStartUtc              DATETIME2(3)     NOT NULL,
        PeriodEndUtc                 DATETIME2(3)     NOT NULL,
        MeanCognitiveLoadScore       DECIMAL(5,2)     NOT NULL,
        OverloadEventCount            INT              NOT NULL,
        ReadingCount                   INT              NOT NULL,
        RecalculatedAtUtc               DATETIME2(3)     NOT NULL,
        RecalculatedBy                   UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_WorkloadTrendRecord_Ledger ON [HumanFactorsCognitiveLoadAnalytics].[WorkloadTrendRecord] (CognitiveLoadLedgerId, RecalculatedAtUtc);
END
GO

-- ---- CognitiveRiskPrediction (separate aggregate) ----------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CognitiveRiskPrediction' AND schema_id = SCHEMA_ID('HumanFactorsCognitiveLoadAnalytics'))
BEGIN
    CREATE TABLE [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction]
    (
        CognitiveRiskPredictionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CognitiveRiskPrediction PRIMARY KEY,
        TenantId                   UNIQUEIDENTIFIER NOT NULL,
        SiteId                     UNIQUEIDENTIFIER NULL,
        WorkerUserId                UNIQUEIDENTIFIER NOT NULL,
        SourceLedgerId               UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CognitiveRiskPrediction_Ledger REFERENCES [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger] (CognitiveLoadLedgerId),
        PredictedRiskLevel            NVARCHAR(20)     NOT NULL,
        PredictionScore                DECIMAL(6,4)     NOT NULL,
        ModelVersion                    NVARCHAR(100)    NOT NULL,
        PredictionBasis                  NVARCHAR(1000)   NOT NULL CONSTRAINT DF_CognitiveRiskPrediction_Basis DEFAULT (N''),
        Status                            NVARCHAR(20)     NOT NULL CONSTRAINT DF_CognitiveRiskPrediction_Status DEFAULT (N'Generated'),
        GeneratedAtUtc                     DATETIME2(3)     NOT NULL,
        GeneratedBy                         UNIQUEIDENTIFIER NOT NULL,
        AcknowledgedAtUtc                    DATETIME2(3)     NULL,
        AcknowledgedBy                        UNIQUEIDENTIFIER NULL,
        AcknowledgementNotes                   NVARCHAR(1000)   NULL,
        IsDeleted                                BIT              NOT NULL CONSTRAINT DF_CognitiveRiskPrediction_IsDeleted DEFAULT (0)
    );

    CREATE NONCLUSTERED INDEX IX_CognitiveRiskPrediction_TenantId_Site ON [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_CognitiveRiskPrediction_Elevated ON [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction] (TenantId, SiteId, PredictedRiskLevel, Status) WHERE IsDeleted = 0;
    CREATE NONCLUSTERED INDEX IX_CognitiveRiskPrediction_SourceLedger ON [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction] (SourceLedgerId);
END
GO
