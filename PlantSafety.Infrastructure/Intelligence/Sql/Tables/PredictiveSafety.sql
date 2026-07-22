-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.PredictiveSafety
--  Layer:          Infrastructure – SQL Scripts
--  File:           PredictiveSafety.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PredictiveSafety read-model, backing
--                  IPredictiveSafetyRepository.GetPredictionsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Intelligence')
BEGIN
    EXEC('CREATE SCHEMA [Intelligence]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PredictiveSafety' AND schema_id = SCHEMA_ID('Intelligence'))
BEGIN
    CREATE TABLE [Intelligence].[PredictiveSafety] (
        PredictionId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId           UNIQUEIDENTIFIER NOT NULL,
        SiteId             UNIQUEIDENTIFIER NULL,
        RiskCategory       NVARCHAR(100)    NOT NULL,
        PredictedRiskScore FLOAT            NOT NULL,
        ModelVersion       NVARCHAR(50)     NULL,
        Status             NVARCHAR(50)     NOT NULL,
        PredictedAtUtc     DATETIME2(3)     NOT NULL,
        GeneratedBy        UNIQUEIDENTIFIER NOT NULL,
        IsDeleted          BIT              NOT NULL CONSTRAINT DF_PredictiveSafety_IsDeleted DEFAULT (0),
        CreatedAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_PredictiveSafety_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy          UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc      DATETIME2(3)     NULL,
        ModifiedBy         UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PredictiveSafety_TenantId_SiteId] ON [Intelligence].[PredictiveSafety] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PredictiveSafety_PredictedAtUtc] ON [Intelligence].[PredictiveSafety] (PredictedAtUtc);
END
GO
