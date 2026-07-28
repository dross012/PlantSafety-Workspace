-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.HazardAssessments
--  Layer:          Infrastructure - SQL Scripts
--  File:           HazardAssessmentsReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the HazardAssessmentsReadModel read-model aggregate backing
--                  IHazardAssessmentsRepository.GetHazardAssessmentsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HazardAssessmentsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[HazardAssessmentsReadModel] (
        [HazardAssessmentId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [HazardDescription]       NVARCHAR(400)       NOT NULL,
        [RiskLevel]               NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [AssessedAtUtc]   DATETIME2(3)        NOT NULL,
        [AssessedBy]      UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_HazardAssessmentsReadModel_TenantId_AssessedAtUtc]
        ON [Operational].[HazardAssessmentsReadModel] ([TenantId], [AssessedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[HazardAssessmentsReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[HazardAssessmentsReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO
