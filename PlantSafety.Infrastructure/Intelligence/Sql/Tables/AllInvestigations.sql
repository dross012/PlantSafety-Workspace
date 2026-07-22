-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.AllInvestigations
--  Layer:          Infrastructure – SQL Scripts
--  File:           AllInvestigations.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the AllInvestigations read-model, backing
--                  IAllInvestigationsRepository.GetInvestigationsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Intelligence')
BEGIN
    EXEC('CREATE SCHEMA [Intelligence]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AllInvestigations' AND schema_id = SCHEMA_ID('Intelligence'))
BEGIN
    CREATE TABLE [Intelligence].[AllInvestigations] (
        InvestigationId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId           UNIQUEIDENTIFIER NOT NULL,
        SiteId             UNIQUEIDENTIFIER NULL,
        IncidentReference  NVARCHAR(100)    NOT NULL,
        AiConfidenceScore  FLOAT            NOT NULL,
        Summary            NVARCHAR(MAX)    NULL,
        Status             NVARCHAR(50)     NOT NULL,
        OpenedAtUtc        DATETIME2(3)     NOT NULL,
        AssignedTo         UNIQUEIDENTIFIER NOT NULL,
        IsDeleted          BIT              NOT NULL CONSTRAINT DF_AllInvestigations_IsDeleted DEFAULT (0),
        CreatedAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_AllInvestigations_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy          UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc      DATETIME2(3)     NULL,
        ModifiedBy         UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AllInvestigations_TenantId_SiteId] ON [Intelligence].[AllInvestigations] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_AllInvestigations_OpenedAtUtc] ON [Intelligence].[AllInvestigations] (OpenedAtUtc);
END
GO
