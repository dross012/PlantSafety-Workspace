-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Administration
--  Layer:          Infrastructure – SQL Scripts
--  File:           Administration.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Administration read-model, backing
--                  IAdministrationRepository.GetRecordsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Administration' AND schema_id = SCHEMA_ID('CrossDomain'))
BEGIN
    CREATE TABLE [CrossDomain].[Administration] (
        RecordId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId           UNIQUEIDENTIFIER NOT NULL,
        SiteId              UNIQUEIDENTIFIER NULL,
        SettingKey         NVARCHAR(200)    NOT NULL,
        SettingValue       NVARCHAR(MAX)    NOT NULL,
        Category            NVARCHAR(100)    NOT NULL,
        Status               NVARCHAR(50)     NOT NULL,
        LastModifiedAtUtc  DATETIME2(3)     NOT NULL,
        LastModifiedBy     UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Administration_TenantId_SiteId] ON [CrossDomain].[Administration] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_Administration_LastModifiedAtUtc] ON [CrossDomain].[Administration] (LastModifiedAtUtc);
END
GO
