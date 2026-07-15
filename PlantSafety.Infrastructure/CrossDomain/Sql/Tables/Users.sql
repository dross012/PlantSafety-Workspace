-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Users
--  Layer:          Infrastructure - SQL Scripts
--  File:           Users.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Users read-model, backing
--                  IUsersRepository.GetUsersAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('CrossDomain'))
BEGIN
    CREATE TABLE [CrossDomain].[Users] (
        UserId             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        DisplayName          NVARCHAR(200)    NOT NULL,
        Email                 NVARCHAR(320)    NOT NULL,
        RoleId                 UNIQUEIDENTIFIER NOT NULL,
        Status                NVARCHAR(50)     NOT NULL,
        LastLoginAtUtc        DATETIME2(3)     NOT NULL,
        CreatedBy             UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Users_TenantId_SiteId ON [CrossDomain].[Users] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX IX_Users_LastLoginAtUtc ON [CrossDomain].[Users] (LastLoginAtUtc);
END
GO
