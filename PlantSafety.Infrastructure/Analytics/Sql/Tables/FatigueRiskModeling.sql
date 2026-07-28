-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Predictive.FatigueRiskModeling
--  Layer:          Infrastructure - SQL Scripts
--  File:           FatigueRiskModeling.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the FatigueRiskModeling read-model aggregate backing
--                  IFatigueRiskModelingRepository.GetFatigueRiskAssessmentsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FatigueRiskModeling' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[FatigueRiskModeling] (
        [RiskAssessmentId]   UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]           UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]             UNIQUEIDENTIFIER    NULL,
        [EmployeeId]         UNIQUEIDENTIFIER    NOT NULL,
        [FatigueRiskScore]   DECIMAL(18,4)       NOT NULL,
        [RiskLevel]          NVARCHAR(50)        NOT NULL,
        [Status]             NVARCHAR(50)        NOT NULL,
        [AssessedAtUtc]      DATETIME2(3)        NOT NULL,
        [RowVersion]         ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_FatigueRiskModeling_TenantId_AssessedAtUtc]
        ON [Analytics].[FatigueRiskModeling] ([TenantId], [AssessedAtUtc])
        INCLUDE ([SiteId], [Status], [RiskLevel]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[FatigueRiskModeling]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Analytics].[FatigueRiskModeling] ADD RowVersion ROWVERSION NOT NULL;
END
GO
