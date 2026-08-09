-- ============================================================================
--  Project:        Shared.SafetyData -- Phase6-MultiFacilityTenant
--  File:           01-table-tenantfacilitymap.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [TenantMap].[TenantFacilityMap] -- a real
--                  many-to-many tenant:facility binding (unlike
--                  [Orchestration].[TenantFacilityRegistration]'s enforced
--                  1:1). Composite PK (TenantId, FacilityId) so one tenant
--                  can be bound to many facilities and vice versa.
--                  IsPrimaryFacility flags at most one row per TenantId as
--                  that tenant's primary facility (enforced by a filtered
--                  unique index below, not a CHECK, since CHECK constraints
--                  cannot reference other rows).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TenantFacilityMap' AND schema_id = SCHEMA_ID('TenantMap'))
BEGIN
    CREATE TABLE [TenantMap].[TenantFacilityMap]
    (
        [TenantId]            NVARCHAR(100)     NOT NULL,
        [FacilityId]          UNIQUEIDENTIFIER  NOT NULL,
        [IsPrimaryFacility]   BIT               NOT NULL CONSTRAINT DF_TenantFacilityMap_IsPrimaryFacility DEFAULT (0),
        [BoundAtUtc]          DATETIME2(3)      NOT NULL,

        [CreatedUtc]          DATETIME2(3)      NOT NULL CONSTRAINT DF_TenantFacilityMap_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]           NVARCHAR(128)     NOT NULL CONSTRAINT DF_TenantFacilityMap_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]          DATETIME2(3)      NULL,
        [UpdatedBy]           NVARCHAR(128)     NULL,
        [IsDeleted]           BIT               NOT NULL CONSTRAINT DF_TenantFacilityMap_IsDeleted DEFAULT (0),

        CONSTRAINT PK_TenantFacilityMap PRIMARY KEY CLUSTERED ([TenantId], [FacilityId]),
        CONSTRAINT FK_TenantFacilityMap_FacilityProfile FOREIGN KEY ([FacilityId])
            REFERENCES [Facilities].[FacilityProfile] ([FacilityId])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_TenantFacilityMap_TenantId_PrimaryFacility' AND object_id = OBJECT_ID('TenantMap.TenantFacilityMap'))
    CREATE UNIQUE NONCLUSTERED INDEX UQ_TenantFacilityMap_TenantId_PrimaryFacility
        ON [TenantMap].[TenantFacilityMap] ([TenantId])
        WHERE [IsPrimaryFacility] = 1 AND [IsDeleted] = 0;
GO
