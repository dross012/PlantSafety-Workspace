-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing (category read-models)
--  Layer:          Infrastructure - SQL Scripts
--  File:           AuditCategoryRecord.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Shared physical read-model table backing the 8 discipline-scoped read
--                  repositories under AuditEngagement/<Discipline>/Repositories (Efficiency,
--                  FacilityAssessment, Inspecting, Operations, Procedures, QualityControl,
--                  RegulatoryCompliance, Systems). Each discipline's read repository queries
--                  this one table filtered by its own [Discipline] discriminator value via a
--                  dedicated usp_<Discipline>_GetItems stored procedure -- see
--                  AuditDisciplineType smart enum for the canonical discipline code list.
--                  Distinct from [Compliance] schema tables (Compliance.Auditing module).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditCategoryRecord' AND schema_id = SCHEMA_ID('Auditing'))
BEGIN
    CREATE TABLE [Auditing].[AuditCategoryRecord] (
        [ItemId]            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [AuditEngagementId] UNIQUEIDENTIFIER NOT NULL,
        [TenantId]          UNIQUEIDENTIFIER NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER NULL,
        [Discipline]        NVARCHAR(100)    NOT NULL,
        [Title]             NVARCHAR(200)    NOT NULL,
        [Status]            NVARCHAR(20)     NOT NULL,
        [RecordedAtUtc]     DATETIME2(3)     NOT NULL,
        [IsDeleted]         BIT              NOT NULL CONSTRAINT DF_AuditCategoryRecord_IsDeleted DEFAULT (0),
        CONSTRAINT FK_AuditCategoryRecord_AuditEngagement
            FOREIGN KEY ([AuditEngagementId]) REFERENCES [Auditing].[AuditEngagement] ([AuditEngagementId])
    );

    CREATE NONCLUSTERED INDEX [IX_AuditCategoryRecord_Discipline_TenantId_RecordedAtUtc]
        ON [Auditing].[AuditCategoryRecord] ([Discipline], [TenantId], [RecordedAtUtc])
        INCLUDE ([SiteId], [Title], [Status], [AuditEngagementId]);
END
GO
