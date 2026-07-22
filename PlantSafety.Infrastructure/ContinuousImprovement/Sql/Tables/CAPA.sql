-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContinuousImprovement.CAPA
--  Layer:          Infrastructure – SQL Scripts
--  File:           CAPA.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the CAPA read-model, backing
--                  ICAPARepository.GetActionsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ContinuousImprovement')
BEGIN
    EXEC('CREATE SCHEMA [ContinuousImprovement]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CAPA' AND schema_id = SCHEMA_ID('ContinuousImprovement'))
BEGIN
    CREATE TABLE [ContinuousImprovement].[CAPA] (
        CapaId        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        SiteId        UNIQUEIDENTIFIER NULL,
        ActionType    NVARCHAR(20)     NOT NULL,
        Title         NVARCHAR(200)    NOT NULL,
        Status        NVARCHAR(50)     NOT NULL,
        DueDateUtc    DATETIME2(3)     NOT NULL,
        AssignedTo    UNIQUEIDENTIFIER NOT NULL,
        IsDeleted     BIT              NOT NULL CONSTRAINT DF_CAPA_IsDeleted DEFAULT (0),
        CreatedAtUtc  DATETIME2(3)     NOT NULL CONSTRAINT DF_CAPA_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy     UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc DATETIME2(3)     NULL,
        ModifiedBy    UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CAPA_TenantId_SiteId] ON [ContinuousImprovement].[CAPA] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CAPA_DueDateUtc] ON [ContinuousImprovement].[CAPA] (DueDateUtc);
END
GO
