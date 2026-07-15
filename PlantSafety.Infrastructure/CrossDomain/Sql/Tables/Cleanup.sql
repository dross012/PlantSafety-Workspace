-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Cleanup
--  Layer:          Infrastructure - SQL Scripts
--  File:           Cleanup.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Cleanup read-model, backing
--                  ICleanupRepository.GetCleanupTasksAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Cleanup' AND schema_id = SCHEMA_ID('CrossDomain'))
BEGIN
    CREATE TABLE [CrossDomain].[Cleanup] (
        CleanupTaskId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteId             UNIQUEIDENTIFIER NULL,
        TargetArea        NVARCHAR(100)    NOT NULL,
        ItemsRemoved       INT              NOT NULL,
        ReclaimedBytes     BIGINT           NOT NULL,
        Status              NVARCHAR(50)     NOT NULL,
        ExecutedAtUtc      DATETIME2(3)     NOT NULL,
        ExecutedBy         UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Cleanup_TenantId_SiteId ON [CrossDomain].[Cleanup] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX IX_Cleanup_ExecutedAtUtc ON [CrossDomain].[Cleanup] (ExecutedAtUtc);
END
GO
