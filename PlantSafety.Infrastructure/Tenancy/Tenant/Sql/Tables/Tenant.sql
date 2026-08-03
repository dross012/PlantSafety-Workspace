-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Tenancy.Tenant
--  Layer:          Infrastructure -- SQL Scripts
--  File:           Tenant.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
--  SchemaVersion:  2026.1
--  Description:    Tables backing the Tenant aggregate root and its supporting provisioning
--                  records ([Tenancy].[TenantApiKey], [Tenancy].[TenantAdminRoleAssignment]).
--                  Lives in a new [Tenancy] schema in PlantSafety.CoreDB.
--
--                  RLS NOTE (see docs/Phase7/tenant-isolation-model.md): [Tenancy].[Tenant] is
--                  DELIBERATELY EXCLUDED from the platform's Row-Level-Security policy
--                  (90-row-level-security.sql) -- its own TenantId column IS the row's identity,
--                  not a foreign reference to "which tenant owns this row" the way every other
--                  RLS-protected table's TenantId column is. Applying the FILTER/BLOCK predicate
--                  here would mean no session could ever list more than the single tenant its own
--                  SESSION_CONTEXT happens to be set to -- exactly the opposite of what a
--                  platform-admin tenant registry needs. This is the same category of exclusion
--                  already applied to AuditLog/ErrorLog/ArchivalJob (shared, cross-tenant
--                  platform-operations tables) -- access control for this table is enforced at
--                  the Application/API layer instead (PlatformAdmin-tier authorization on
--                  TenantV1Controller), not via RLS.
--                  [Tenancy].[TenantApiKey] and [Tenancy].[TenantAdminRoleAssignment] ARE
--                  genuinely tenant-scoped (their TenantId column references an owning tenant,
--                  not their own identity), so they ARE picked up automatically by
--                  90-row-level-security.sql's live-discovery query -- no changes needed there.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Tenancy')
BEGIN
    EXEC('CREATE SCHEMA [Tenancy]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Tenant' AND schema_id = SCHEMA_ID('Tenancy'))
BEGIN
    CREATE TABLE [Tenancy].[Tenant] (
        [TenantId]          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name]              NVARCHAR(200)    NOT NULL,
        [Region]            NVARCHAR(50)     NOT NULL,
        [Status]            NVARCHAR(20)     NOT NULL,
        [PlanCode]          NVARCHAR(30)     NOT NULL,
        [ConfigurationJson] NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_Tenant_Configuration DEFAULT (N'{}'),
        [CreatedAtUtc]      DATETIME2(3)     NOT NULL,
        [ActivatedAtUtc]    DATETIME2(3)     NULL,
        [SuspendedAtUtc]    DATETIME2(3)     NULL,
        [SuspensionReason]  NVARCHAR(2000)   NULL,
        [ArchivedAtUtc]     DATETIME2(3)     NULL,
        [DeletedAtUtc]      DATETIME2(3)     NULL,
        [DeletionReason]    NVARCHAR(2000)   NULL
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_Tenant_Name ON [Tenancy].[Tenant] (Name)
        WHERE [DeletedAtUtc] IS NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TenantApiKey' AND schema_id = SCHEMA_ID('Tenancy'))
BEGIN
    CREATE TABLE [Tenancy].[TenantApiKey] (
        [TenantApiKeyId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]       UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_TenantApiKey_Tenant REFERENCES [Tenancy].[Tenant] (TenantId),
        [KeyHash]        NVARCHAR(128)    NOT NULL,
        [KeyPrefix]      NVARCHAR(20)     NOT NULL,
        [CreatedAtUtc]   DATETIME2(3)     NOT NULL,
        [RevokedAtUtc]   DATETIME2(3)     NULL
    );

    CREATE NONCLUSTERED INDEX IX_TenantApiKey_Tenant ON [Tenancy].[TenantApiKey] (TenantId, CreatedAtUtc DESC);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TenantAdminRoleAssignment' AND schema_id = SCHEMA_ID('Tenancy'))
BEGIN
    CREATE TABLE [Tenancy].[TenantAdminRoleAssignment] (
        [TenantAdminRoleAssignmentId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [TenantId]           UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_TenantAdminRoleAssignment_Tenant REFERENCES [Tenancy].[Tenant] (TenantId),
        [UserId]             UNIQUEIDENTIFIER NOT NULL,
        [GrantedRole]        NVARCHAR(50)     NOT NULL,
        [InheritedRoleNames] NVARCHAR(500)    NOT NULL,
        [CorrelationId]      UNIQUEIDENTIFIER NOT NULL,
        [GrantedAtUtc]       DATETIME2(3)     NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_TenantAdminRoleAssignment_Tenant ON [Tenancy].[TenantAdminRoleAssignment] (TenantId, GrantedAtUtc DESC);
END
GO
