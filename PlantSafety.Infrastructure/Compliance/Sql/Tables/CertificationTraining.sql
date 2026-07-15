-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Certification.Training
--  Layer:          Infrastructure – SQL Scripts
--  File:           CertificationTraining.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Certification.Training read-model, backing
--                  ITrainingRepository.GetTrainingAsync. Named with the
--                  Certification prefix to avoid collision with the unrelated
--                  Compliance.RegulatoryCalendar.Renewals.Training aggregate.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationTraining' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationTraining] (
        TrainingRecordId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        EmployeeName     NVARCHAR(200)    NOT NULL,
        CourseName       NVARCHAR(200)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        CompletedAtUtc   DATETIME2(3)     NOT NULL,
        ExpiresAtUtc     DATETIME2(3)     NOT NULL,
        InstructorId     UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationTraining_TenantId_SiteId] ON [Compliance].[CertificationTraining] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationTraining_CompletedAtUtc] ON [Compliance].[CertificationTraining] (CompletedAtUtc);
END
GO
