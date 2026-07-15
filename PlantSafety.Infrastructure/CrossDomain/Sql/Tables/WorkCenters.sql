-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.WorkCenters
--  Layer:          Infrastructure - SQL Scripts
--  File:           WorkCenters.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the WorkCenters read-model, backing
--                  IWorkCentersRepository.GetWorkCentersAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkCenters' AND schema_id = SCHEMA_ID('CrossDomain'))
BEGIN
    CREATE TABLE [CrossDomain].[WorkCenters] (
        WorkCenterId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        WorkCenterName        NVARCHAR(200)    NOT NULL,
        WorkCenterCode          NVARCHAR(50)     NOT NULL,
        Capacity                 INT              NOT NULL,
        Status                  NVARCHAR(50)     NOT NULL,
        LastUpdatedAtUtc        DATETIME2(3)     NOT NULL,
        LastUpdatedBy           UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_WorkCenters_TenantId_SiteId ON [CrossDomain].[WorkCenters] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX IX_WorkCenters_LastUpdatedAtUtc ON [CrossDomain].[WorkCenters] (LastUpdatedAtUtc);
END
GO
