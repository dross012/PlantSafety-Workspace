-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.JobSafetyAnalysis
--  Layer:          Infrastructure - SQL Scripts
--  File:           JobSafetyAnalysisReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the JobSafetyAnalysisReadModel read-model aggregate backing
--                  IJobSafetyAnalysisRepository.GetJobSafetyAnalysisAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'JobSafetyAnalysisReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[JobSafetyAnalysisReadModel] (
        [JobSafetyAnalysisId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [TaskName]                NVARCHAR(400)       NOT NULL,
        [RiskLevel]               NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [AnalyzedAtUtc]   DATETIME2(3)        NOT NULL,
        [AnalyzedBy]      UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_JobSafetyAnalysisReadModel_TenantId_AnalyzedAtUtc]
        ON [Operational].[JobSafetyAnalysisReadModel] ([TenantId], [AnalyzedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[JobSafetyAnalysisReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[JobSafetyAnalysisReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO
