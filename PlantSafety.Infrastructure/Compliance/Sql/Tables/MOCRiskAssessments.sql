-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange.RiskAssessments
--  Layer:          Infrastructure – SQL Scripts
--  File:           MOCRiskAssessments.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ManagementOfChange.RiskAssessments read-model, backing
--                  IRiskAssessmentsRepository.GetRiskAssessmentsAsync. Named with the
--                  MOC prefix to avoid collision with unrelated RiskAssessments/RiskManagement
--                  aggregates elsewhere in the platform.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MOCRiskAssessments' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[MOCRiskAssessments] (
        RiskAssessmentId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ChangeRequestId  UNIQUEIDENTIFIER NOT NULL,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        RiskLevel        NVARCHAR(50)     NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        AssessedAtUtc    DATETIME2(3)     NOT NULL,
        AssessedBy       UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_MOCRiskAssessments_TenantId_SiteId] ON [Compliance].[MOCRiskAssessments] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_MOCRiskAssessments_AssessedAtUtc] ON [Compliance].[MOCRiskAssessments] (AssessedAtUtc);
    CREATE NONCLUSTERED INDEX [IX_MOCRiskAssessments_ChangeRequestId] ON [Compliance].[MOCRiskAssessments] (ChangeRequestId);
END
GO
