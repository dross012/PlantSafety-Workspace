-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.AIInvestigations
--  Layer:          Infrastructure – SQL Scripts
--  File:           AIInvestigation.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the AIInvestigation aggregate root, backing
--                  IAIInvestigationRepository (Domain write-side interface). Phase 4 full
--                  build-out. Named [Intelligence].[AIInvestigation] (singular) to avoid colliding
--                  with the pre-existing, unrelated read-only [Intelligence].[AllInvestigations]
--                  table backing IAllInvestigationsRepository.GetInvestigationsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Intelligence')
BEGIN
    EXEC('CREATE SCHEMA [Intelligence]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AIInvestigation' AND schema_id = SCHEMA_ID('Intelligence'))
BEGIN
    CREATE TABLE [Intelligence].[AIInvestigation] (
        AIInvestigationId  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId           UNIQUEIDENTIFIER NOT NULL,
        SiteId             UNIQUEIDENTIFIER NULL,
        Status             NVARCHAR(50)     NOT NULL,
        IncidentReference  NVARCHAR(100)    NOT NULL,
        AiConfidenceScore  FLOAT            NOT NULL,
        Summary            NVARCHAR(MAX)    NULL,
        AssignedTo         UNIQUEIDENTIFIER NOT NULL,
        OpenedAtUtc        DATETIME2(3)     NOT NULL,
        ReviewStartedBy    UNIQUEIDENTIFIER NULL,
        ReviewStartedAtUtc DATETIME2(3)     NULL,
        ClosedBy           UNIQUEIDENTIFIER NULL,
        ResolutionSummary  NVARCHAR(MAX)    NULL,
        ClosedAtUtc        DATETIME2(3)     NULL,
        ArchivedAtUtc      DATETIME2(3)     NULL,
        CreatedAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_AIInvestigation_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy          UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc       DATETIME2(3)     NULL,
        UpdatedBy          UNIQUEIDENTIFIER NULL,
        IsDeleted          BIT              NOT NULL CONSTRAINT DF_AIInvestigation_IsDeleted DEFAULT (0),
        RowVersion         ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AIInvestigation_TenantId_SiteId] ON [Intelligence].[AIInvestigation] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_AIInvestigation_OpenedAtUtc] ON [Intelligence].[AIInvestigation] (OpenedAtUtc);
END
GO
