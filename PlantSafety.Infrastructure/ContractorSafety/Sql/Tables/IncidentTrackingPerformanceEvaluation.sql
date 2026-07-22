-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
--  Layer:          Infrastructure - SQL Scripts
--  File:           IncidentTrackingPerformanceEvaluation.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the IncidentTrackingPerformanceEvaluation aggregate root backing IIncidentTrackingPerformanceEvaluationRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ContractorSafety')
BEGIN
    EXEC('CREATE SCHEMA [ContractorSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IncidentTrackingPerformanceEvaluation' AND schema_id = SCHEMA_ID('ContractorSafety'))
BEGIN
    CREATE TABLE [ContractorSafety].[IncidentTrackingPerformanceEvaluation] (
        [IncidentTrackingPerformanceEvaluationId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [EvaluationTitle] NVARCHAR(400) NOT NULL,
        [IncidentCategory] NVARCHAR(100) NOT NULL,
        [ContractorName] NVARCHAR(400) NOT NULL,
        [EvaluatedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [InvestigatedAtUtc] DATETIME2(3) NULL,
        [RootCauseSummary] NVARCHAR(2000) NULL,
        [CapaCount] INT NOT NULL CONSTRAINT DF_IncidentTrackingPerformanceEvaluation_CapaCount DEFAULT (0),
        [PerformanceScore] DECIMAL(9,2) NULL,
        [ClosedAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_IncidentTrackingPerformanceEvaluation_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_IncidentTrackingPerformanceEvaluation_TenantId_SiteId]
        ON [ContractorSafety].[IncidentTrackingPerformanceEvaluation] ([TenantId], [SiteId])
        INCLUDE ([Status], [EvaluationTitle]);
END
GO
