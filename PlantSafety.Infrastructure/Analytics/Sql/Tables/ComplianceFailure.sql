-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Predictive.ComplianceFailure
--  Layer:          Infrastructure - SQL Scripts
--  File:           ComplianceFailure.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ComplianceFailure read-model aggregate backing
--                  IComplianceFailureRepository.GetComplianceFailurePredictionsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComplianceFailure' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[ComplianceFailure] (
        [PredictionId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]           UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]             UNIQUEIDENTIFIER    NULL,
        [ComplianceArea]     NVARCHAR(200)       NOT NULL,
        [FailureProbability] DECIMAL(18,4)       NOT NULL,
        [RiskLevel]          NVARCHAR(50)        NOT NULL,
        [Status]             NVARCHAR(50)        NOT NULL,
        [PredictedAtUtc]     DATETIME2(3)        NOT NULL,
        [RowVersion]         ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ComplianceFailure_TenantId_PredictedAtUtc]
        ON [Analytics].[ComplianceFailure] ([TenantId], [PredictedAtUtc])
        INCLUDE ([SiteId], [Status], [RiskLevel]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[ComplianceFailure]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Analytics].[ComplianceFailure] ADD RowVersion ROWVERSION NOT NULL;
END
GO
