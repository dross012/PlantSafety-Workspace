-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.EnterpriseFabric.Users
--  Layer:          Infrastructure – SQL Scripts
--  File:           User.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the [EnterpriseFabric].[User] aggregate, backing
--                  IUserRepository / UserDapperRepository (PlantSafety.Infrastructure.
--                  Features.EnterpriseFabric.Users.Repositories). Column list mirrors
--                  UserReadModel (PlantSafety.Application.Features.EnterpriseFabric.Handlers.
--                  Users.IUserRepository) and the StoredProcedureBuilder Add* calls in
--                  UserDapperRepository.CreateAsync exactly.
--
--  DESIGN NOTES: Roles and SiteIds are persisted as delimited NVARCHAR(4000) columns
--  (RolesCsv/SiteIdsCsv) because UserDapperRepository.CreateAsync already joins them with
--  string.Join(',', ...) before sending a single string parameter -- no repository-side
--  change needed to consume a plain CSV column. IUserRepository currently exposes no
--  delete/deactivate operation (its only mutation is CreateAsync), so no IsDeleted/soft-delete
--  columns are included; add them if/when a delete path is introduced. UNIQUE (TenantId, Email)
--  is a correctness addition beyond what the C# layer enforces today -- not derived from any
--  Add* call, but a per-tenant duplicate-email User row would be a data-integrity defect this
--  table should not allow. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnterpriseFabric')
BEGIN
    EXEC('CREATE SCHEMA [EnterpriseFabric]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'User' AND schema_id = SCHEMA_ID('EnterpriseFabric'))
BEGIN
    CREATE TABLE [EnterpriseFabric].[User] (
        UserId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_User PRIMARY KEY,
        TenantId           UNIQUEIDENTIFIER NOT NULL,
        Email              NVARCHAR(320)    NOT NULL,
        FirstName          NVARCHAR(100)    NOT NULL,
        LastName           NVARCHAR(100)    NOT NULL,
        RolesCsv           NVARCHAR(4000)   NULL,
        SiteIdsCsv         NVARCHAR(4000)   NULL,
        IsActive           BIT              NOT NULL CONSTRAINT DF_User_IsActive DEFAULT (1),
        CreatedAtUtc       DATETIME2(3)     NOT NULL,
        CreatedBy          UNIQUEIDENTIFIER NOT NULL,
        LastModifiedAtUtc  DATETIME2(3)     NOT NULL,
        RowVersion         ROWVERSION       NOT NULL,

        CONSTRAINT UQ_User_TenantId_Email UNIQUE (TenantId, Email)
    );

    CREATE NONCLUSTERED INDEX IX_User_TenantId ON [EnterpriseFabric].[User] (TenantId);
    CREATE NONCLUSTERED INDEX IX_User_TenantId_IsActive ON [EnterpriseFabric].[User] (TenantId, IsActive);
END
GO
