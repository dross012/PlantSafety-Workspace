-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Roles
--  Layer:          Infrastructure - SQL Scripts
--  File:           Roles.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Roles read-model, backing
--                  IRolesRepository.GetRolesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Roles' AND schema_id = SCHEMA_ID('CrossDomain'))
BEGIN
    CREATE TABLE [CrossDomain].[Roles] (
        RoleId             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        RoleName            NVARCHAR(200)    NOT NULL,
        Description           NVARCHAR(1000)   NOT NULL,
        PermissionCount        INT              NOT NULL,
        Status                NVARCHAR(50)     NOT NULL,
        CreatedAtUtc          DATETIME2(3)     NOT NULL,
        CreatedBy             UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Roles_TenantId_SiteId ON [CrossDomain].[Roles] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX IX_Roles_CreatedAtUtc ON [CrossDomain].[Roles] (CreatedAtUtc);
END
GO
