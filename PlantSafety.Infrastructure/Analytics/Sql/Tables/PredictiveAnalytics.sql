-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.PredictiveAnalytics
--  Layer:          Infrastructure - SQL Scripts
--  File:           PredictiveAnalytics.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PredictiveAnalytics read-model aggregate backing
--                  IPredictiveAnalyticsRepository.GetPredictionsAsync.
--                  Additive-only evolution.
--  UPDATED:        2026-08-02 -- Phase 4 full build-out: additively extended this same table
--                  (rather than introducing a second, colliding table) to also back the
--                  Domain.Analytics.PredictiveAnalytics.Interfaces.IPredictiveAnalyticsRepository
--                  write-side aggregate repository. [PredictionId] doubles as the aggregate's
--                  PredictiveAnalyticsId (already the PK); [GeneratedAtUtc]/[GeneratedBy] double
--                  as the aggregate's CreatedAtUtc/CreatedBy (the Create factory sets
--                  GeneratedAtUtc = createdAtUtc); [Score] doubles as the aggregate's
--                  PredictedRiskScore. New columns (PredictionType, TargetEntityId,
--                  ConfidenceLevel, PredictionHorizonDays, UpdatedAtUtc, UpdatedBy, IsDeleted) are
--                  added guarded by IF NOT EXISTS column checks, mirroring the pre-existing
--                  RowVersion retrofit pattern immediately below. usp_PredictiveAnalytics_List's
--                  explicit column list is unaffected by this addition.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PredictiveAnalytics' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[PredictiveAnalytics] (
        [PredictionId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ModelName]         NVARCHAR(200)       NOT NULL,
        [Score]             DECIMAL(18,4)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [GeneratedAtUtc]    DATETIME2(3)        NOT NULL,
        [GeneratedBy]       UNIQUEIDENTIFIER    NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PredictiveAnalytics_TenantId_GeneratedAtUtc]
        ON [Analytics].[PredictiveAnalytics] ([TenantId], [GeneratedAtUtc])
        INCLUDE ([SiteId], [Status], [ModelName]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[PredictiveAnalytics]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD RowVersion ROWVERSION NOT NULL;
END
GO

-- Phase 4 full build-out: additive columns for the write-side aggregate repository.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[PredictiveAnalytics]') AND name = 'PredictionType')
BEGIN
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD PredictionType NVARCHAR(200) NOT NULL CONSTRAINT DF_PredictiveAnalytics_PredictionType DEFAULT (N'');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[PredictiveAnalytics]') AND name = 'TargetEntityId')
BEGIN
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD TargetEntityId UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[PredictiveAnalytics]') AND name = 'ConfidenceLevel')
BEGIN
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD ConfidenceLevel DECIMAL(18,4) NOT NULL CONSTRAINT DF_PredictiveAnalytics_ConfidenceLevel DEFAULT (0);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[PredictiveAnalytics]') AND name = 'PredictionHorizonDays')
BEGIN
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD PredictionHorizonDays INT NOT NULL CONSTRAINT DF_PredictiveAnalytics_PredictionHorizonDays DEFAULT (30);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[PredictiveAnalytics]') AND name = 'UpdatedAtUtc')
BEGIN
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD UpdatedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[PredictiveAnalytics]') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD UpdatedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[PredictiveAnalytics]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD IsDeleted BIT NOT NULL CONSTRAINT DF_PredictiveAnalytics_IsDeleted DEFAULT (0);
END
GO
