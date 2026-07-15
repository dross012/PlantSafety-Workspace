-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Backup
--  Layer:          Infrastructure - SQL Scripts
--  File:           Backup.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Backup read-model, backing
--                  IBackupRepository.GetBackupJobsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Backup' AND schema_id = SCHEMA_ID('CrossDomain'))
BEGIN
    CREATE TABLE [CrossDomain].[Backup] (
        BackupJobId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteId             UNIQUEIDENTIFIER NULL,
        BackupType        NVARCHAR(50)     NOT NULL,
        TargetLocation    NVARCHAR(500)    NOT NULL,
        SizeBytes          BIGINT           NOT NULL,
        Status              NVARCHAR(50)     NOT NULL,
        CompletedAtUtc     DATETIME2(3)     NOT NULL,
        InitiatedBy        UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Backup_TenantId_SiteId ON [CrossDomain].[Backup] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX IX_Backup_CompletedAtUtc ON [CrossDomain].[Backup] (CompletedAtUtc);
END
GO
