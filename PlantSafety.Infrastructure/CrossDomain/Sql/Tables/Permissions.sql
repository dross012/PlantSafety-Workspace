-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Permissions
--  Layer:          Infrastructure - SQL Scripts
--  File:           Permissions.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permissions read-model, backing
--                  IPermissionsRepository.GetPermissionsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Permissions' AND schema_id = SCHEMA_ID('CrossDomain'))
BEGIN
    CREATE TABLE [CrossDomain].[Permissions] (
        PermissionId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        PermissionName      NVARCHAR(200)    NOT NULL,
        Scope                 NVARCHAR(50)     NOT NULL,
        GrantedToRoleId       UNIQUEIDENTIFIER NOT NULL,
        Status                NVARCHAR(50)     NOT NULL,
        GrantedAtUtc          DATETIME2(3)     NOT NULL,
        GrantedBy             UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Permissions_TenantId_SiteId ON [CrossDomain].[Permissions] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX IX_Permissions_GrantedAtUtc ON [CrossDomain].[Permissions] (GrantedAtUtc);
END
GO
