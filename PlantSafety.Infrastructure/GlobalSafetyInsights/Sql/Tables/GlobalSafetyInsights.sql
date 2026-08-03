-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         GlobalSafetyInsights
--  Layer:          Infrastructure -- SQL Scripts
--  File:           GlobalSafetyInsights.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 8 §2/§4 Multi-Site Safety Intelligence Network)
--  SchemaVersion:  2026.1
--  Description:    Tables backing the GlobalSafetyInsightsAggregate (append-only computed
--                  snapshots) and the CrossSiteActionEvent audit ledger (Phase 8 §4 event-mesh
--                  extension). TenantId NOT NULL uniqueidentifier on both tables so the platform-
--                  wide Row-Level-Security discovery in 90-row-level-security.sql picks them up
--                  automatically -- zero cross-tenant leakage. Child collections (SiteIds,
--                  RiskScores, Anomalies, Recommendations) are stored as NVARCHAR(MAX) JSON columns
--                  rather than separate child tables: this aggregate is an immutable, append-only,
--                  point-in-time analytics snapshot (never edited after insert), so the
--                  upsert-reconciliation pattern used by mutable aggregates (e.g. Accident's
--                  CorrectiveActions) would add complexity with no benefit. Additive-only
--                  evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'GlobalSafetyInsights')
    EXEC('CREATE SCHEMA GlobalSafetyInsights AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Snapshot' AND schema_id = SCHEMA_ID('GlobalSafetyInsights'))
BEGIN
    CREATE TABLE [GlobalSafetyInsights].[Snapshot]
    (
        GlobalSafetyInsightsId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_GlobalSafetyInsights_Snapshot PRIMARY KEY,
        TenantId                         UNIQUEIDENTIFIER NOT NULL,
        RegionCode                       NVARCHAR(30)     NOT NULL,
        SiteIdsJson                      NVARCHAR(MAX)    NOT NULL,
        WindowStartUtc                   DATETIME2(3)     NOT NULL,
        WindowEndUtc                     DATETIME2(3)     NOT NULL,
        Trir                             DECIMAL(18,4)    NOT NULL,
        Ltir                             DECIMAL(18,4)    NOT NULL,
        ExposureRate                     DECIMAL(18,4)    NOT NULL,
        IncidentDensity                  DECIMAL(18,6)    NOT NULL,
        MachineStateIncidentCorrelation  FLOAT            NOT NULL,
        TotalRecordableIncidents         INT              NOT NULL,
        TotalLostTimeIncidents           INT              NOT NULL,
        TotalHoursWorked                 DECIMAL(18,2)    NOT NULL,
        RiskScoresJson                   NVARCHAR(MAX)    NOT NULL,
        AnomaliesJson                    NVARCHAR(MAX)    NOT NULL,
        RecommendationsJson              NVARCHAR(MAX)    NOT NULL,
        ComputedAtUtc                    DATETIME2(3)     NOT NULL,
        CreatedBy                        UNIQUEIDENTIFIER NOT NULL,
        IsDeleted                        BIT              NOT NULL CONSTRAINT DF_GlobalSafetyInsights_Snapshot_IsDeleted DEFAULT (0),
        RowVersion                       ROWVERSION
    );

    CREATE NONCLUSTERED INDEX IX_GlobalSafetyInsights_Snapshot_TenantId_ComputedAtUtc
        ON [GlobalSafetyInsights].[Snapshot] (TenantId, ComputedAtUtc DESC) INCLUDE (IsDeleted);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CrossSiteActionLog' AND schema_id = SCHEMA_ID('GlobalSafetyInsights'))
BEGIN
    CREATE TABLE [GlobalSafetyInsights].[CrossSiteActionLog]
    (
        CrossSiteActionLogId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_GlobalSafetyInsights_CrossSiteActionLog PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        AffectedSiteIdsJson   NVARCHAR(MAX)    NOT NULL,
        ActionType            NVARCHAR(100)    NOT NULL,
        Description           NVARCHAR(2000)   NULL,
        InitiatedByUserId     UNIQUEIDENTIFIER NULL,
        RecordedAtUtc         DATETIME2(3)     NOT NULL,
        RegionCode            NVARCHAR(20)     NULL
    );

    CREATE NONCLUSTERED INDEX IX_GlobalSafetyInsights_CrossSiteActionLog_TenantId_RecordedAtUtc
        ON [GlobalSafetyInsights].[CrossSiteActionLog] (TenantId, RecordedAtUtc DESC);
END
GO

-- Phase 8 §6 governance-validation fix (2026-08-03): RegionCode was resolved by
-- MultiSiteActionRouter but discarded before this table existed with a place for it. Additive
-- column for pre-existing deployments that already ran the CREATE TABLE block above.
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CrossSiteActionLog' AND schema_id = SCHEMA_ID('GlobalSafetyInsights'))
   AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[GlobalSafetyInsights].[CrossSiteActionLog]') AND name = 'RegionCode')
BEGIN
    ALTER TABLE [GlobalSafetyInsights].[CrossSiteActionLog] ADD RegionCode NVARCHAR(20) NULL;
END
GO
