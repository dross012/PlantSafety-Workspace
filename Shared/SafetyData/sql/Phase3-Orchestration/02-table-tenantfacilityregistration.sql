-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase3-Orchestration
--  Layer:          Infrastructure - SQL Scripts
--  File:           02-table-tenantfacilityregistration.sql
--  Created:        2026-08-08
--  Author:         Platform Team (schema-sentinel-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [Orchestration].[TenantFacilityRegistration]
--                  -- the persisted mirror of Shared.SafetyData.Abstractions.
--                  Models.Industries.Orchestration.TenantFacilityRegistration
--                  (TenantFacilityRegistration.cs), the current-state 1
--                  tenant : 1 facility binding maintained by
--                  Shared.SafetyData.Infrastructure.Industries.
--                  TenantFacilityRegistry.
--
--  StrongId / PK: [TenantId] (NVARCHAR) -- this is a current-state registry,
--  not event history: ITenantFacilityRegistry.Register upserts (replaces) by
--  TenantId (ConcurrentDictionary<string, TenantFacilityRegistration> keyed
--  by TenantId in the C# implementation), so TenantId -- not a synthetic
--  surrogate -- is the table's true natural/strong identity, matching
--  22-usp-tenantfacilityregistration-register.sql's MERGE ... ON
--  target.[TenantId] = source.TenantId. FacilityId carries its own UNIQUE
--  constraint to preserve the "1 tenant : 1 facility" binding described in
--  the C# record's own header.
--
--  CANONICAL METADATA NOTE: mutable, tenant-scoped registry data (unlike
--  [SafetyTaxonomy]'s static reference tables), so it carries the full
--  canonical metadata set. CreatedBy/UpdatedBy/IsDeleted carry DEFAULT
--  constraints so that 22-usp-tenantfacilityregistration-register.sql's
--  MERGE (which supplies CreatedUtc/UpdatedUtc explicitly but omits
--  CreatedBy/UpdatedBy/IsDeleted) still succeeds.
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TenantFacilityRegistration' AND schema_id = SCHEMA_ID('Orchestration'))
BEGIN
    CREATE TABLE [Orchestration].[TenantFacilityRegistration]
    (
        [TenantId]        NVARCHAR(100)     NOT NULL,
        [FacilityId]      UNIQUEIDENTIFIER  NOT NULL,
        [Name]            NVARCHAR(200)     NOT NULL,
        [IndustryName]    NVARCHAR(200)     NOT NULL,
        [Location]        NVARCHAR(400)     NOT NULL,

        [CreatedUtc]      DATETIME2(3)      NOT NULL CONSTRAINT DF_TenantFacilityRegistration_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]       NVARCHAR(128)     NOT NULL CONSTRAINT DF_TenantFacilityRegistration_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]      DATETIME2(3)      NULL,
        [UpdatedBy]       NVARCHAR(128)     NULL,
        [IsDeleted]       BIT               NOT NULL CONSTRAINT DF_TenantFacilityRegistration_IsDeleted DEFAULT (0),

        CONSTRAINT PK_TenantFacilityRegistration PRIMARY KEY CLUSTERED ([TenantId]),
        CONSTRAINT UQ_TenantFacilityRegistration_FacilityId UNIQUE ([FacilityId])
    );
END
GO
