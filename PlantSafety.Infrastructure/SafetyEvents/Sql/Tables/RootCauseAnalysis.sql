-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.RootCauseAnalysis
--  Layer:          Infrastructure – SQL Scripts
--  File:           RootCauseAnalysis.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the RootCauseAnalysis read-model, backing
--                  IRootCauseAnalysisRepository.GetRootCauseAnalysesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RootCauseAnalysis' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[RootCauseAnalysis] (
        AnalysisId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        RelatedEventId  UNIQUEIDENTIFIER NULL,
        Methodology     NVARCHAR(50)     NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        CompletedAtUtc  DATETIME2(3)     NOT NULL,
        LeadAnalyst     UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RootCauseAnalysis_TenantId_SiteId] ON [SafetyEvents].[RootCauseAnalysis] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_RootCauseAnalysis_CompletedAtUtc] ON [SafetyEvents].[RootCauseAnalysis] (CompletedAtUtc);
END
GO
