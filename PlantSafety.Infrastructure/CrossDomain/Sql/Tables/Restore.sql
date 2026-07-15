-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Restore
--  Layer:          Infrastructure - SQL Scripts
--  File:           Restore.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Restore read-model, backing
--                  IRestoreRepository.GetRestoreJobsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Restore' AND schema_id = SCHEMA_ID('CrossDomain'))
BEGIN
    CREATE TABLE [CrossDomain].[Restore] (
        RestoreJobId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        SourceBackupId       UNIQUEIDENTIFIER NOT NULL,
        RestoreType           NVARCHAR(50)     NOT NULL,
        TargetLocation        NVARCHAR(500)    NOT NULL,
        Status                NVARCHAR(50)     NOT NULL,
        CompletedAtUtc        DATETIME2(3)     NOT NULL,
        InitiatedBy           UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Restore_TenantId_SiteId ON [CrossDomain].[Restore] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX IX_Restore_CompletedAtUtc ON [CrossDomain].[Restore] (CompletedAtUtc);
END
GO
