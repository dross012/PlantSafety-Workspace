-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.DataLake
--  Layer:          Infrastructure – SQL Scripts
--  File:           DataLake.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the DataLake read-model, backing
--                  IDataLakeRepository.GetDataLakeRecordsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Intelligence')
BEGIN
    EXEC('CREATE SCHEMA [Intelligence]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DataLake' AND schema_id = SCHEMA_ID('Intelligence'))
BEGIN
    CREATE TABLE [Intelligence].[DataLake] (
        DataLakeRecordId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        DatasetName      NVARCHAR(200)    NOT NULL,
        Zone             NVARCHAR(50)     NOT NULL,
        RecordCount      BIGINT           NULL,
        Status           NVARCHAR(50)     NOT NULL,
        ProcessedAtUtc   DATETIME2(3)     NOT NULL,
        ProcessedBy      UNIQUEIDENTIFIER NOT NULL,
        IsDeleted        BIT              NOT NULL CONSTRAINT DF_DataLake_IsDeleted DEFAULT (0),
        CreatedAtUtc     DATETIME2(3)     NOT NULL CONSTRAINT DF_DataLake_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy        UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc    DATETIME2(3)     NULL,
        ModifiedBy       UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DataLake_TenantId_SiteId] ON [Intelligence].[DataLake] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_DataLake_ProcessedAtUtc] ON [Intelligence].[DataLake] (ProcessedAtUtc);
END
GO
