-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContinuousImprovement.RCA
--  Layer:          Infrastructure – SQL Scripts
--  File:           RCA.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the RCA read-model, backing
--                  IRCARepository.GetRcaRecordsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ContinuousImprovement')
BEGIN
    EXEC('CREATE SCHEMA [ContinuousImprovement]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RCA' AND schema_id = SCHEMA_ID('ContinuousImprovement'))
BEGIN
    CREATE TABLE [ContinuousImprovement].[RCA] (
        RcaId             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteId            UNIQUEIDENTIFIER NULL,
        ProblemStatement  NVARCHAR(MAX)    NOT NULL,
        Methodology       NVARCHAR(50)     NOT NULL,
        Status            NVARCHAR(50)     NOT NULL,
        InitiatedAtUtc    DATETIME2(3)     NOT NULL,
        OwnerUserId       UNIQUEIDENTIFIER NOT NULL,
        IsDeleted         BIT              NOT NULL CONSTRAINT DF_RCA_IsDeleted DEFAULT (0),
        CreatedAtUtc      DATETIME2(3)     NOT NULL CONSTRAINT DF_RCA_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy         UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc     DATETIME2(3)     NULL,
        ModifiedBy        UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RCA_TenantId_SiteId] ON [ContinuousImprovement].[RCA] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_RCA_InitiatedAtUtc] ON [ContinuousImprovement].[RCA] (InitiatedAtUtc);
END
GO
