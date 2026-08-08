-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase4-Facilities
--  Layer:          Infrastructure - SQL Scripts
--  File:           01-table-facilityprofile.sql
--  Created:        2026-08-08
--  Author:         Platform Team (schema-sentinel-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [Facilities].[FacilityProfile] -- the SQL
--                  read-model cache of Shared.SafetyData.Abstractions.Models.
--                  Industries.Facilities.FacilityProfile (FacilityProfile.cs),
--                  built exclusively by FacilityProfileBuilder.Build. One row
--                  per FacilityId, upserted whenever the profile is
--                  recomputed (e.g. after a TenantFacilityRegistration change
--                  or a completed-elements update), via
--                  20-usp-facilityprofile-upsert.sql.
--
--  SCOPE NOTE (mirrors FacilityProfile.cs's own SCOPE NOTE and
--  00-create-schema.sql's): FacilityId/Name/Location are caller-supplied,
--  not an identity Shared.SafetyData owns or persists elsewhere -- this
--  table is a read-model CACHE of FacilityProfileBuilder.Build's most recent
--  output, not the authoritative Facility identity store. FacilityId is
--  still this table's StrongId clustered PK (it is the stable key every
--  caller/consumer already addresses a facility by -- see
--  [Orchestration].[TenantFacilityRegistration].[FacilityId] and
--  [Orchestration].[FacilityEventOrchestrationResult].[FacilityId]).
--
--  COLUMN SHAPE: Regulators/RequiredPrograms/RequiredPermits/
--  RequiredTraining/RequiredAudits/RequiredSops (all IReadOnlyList<T> in the
--  C# record) are deliberately NOT persisted as columns on this table --
--  every one is a pure, deterministic derivation of IndustryName alone (via
--  IndustryRegistry / ProgramRequirementMapping / the Consumers.* catalogs,
--  already fully queryable through [SafetyTaxonomy]), so caching them here
--  would be redundant, doubly-mutable state. Only the caller-supplied
--  scalar identity fields (Name/IndustryName/Location) and the raw Flags
--  bitmask are persisted -- matches
--  20-usp-facilityprofile-upsert.sql's own column list exactly (this table's
--  DDL is grounded in that already-authored proc, not designed
--  independently of it).
--
--  Flags is FacilityProfile's raw [Flags]-enum SafetyFlag bitmask, stored as
--  BIGINT (not INT) to match 20-usp-facilityprofile-upsert.sql's
--  @Flags bigint parameter -- SafetyTaxonomy's own IndustryFlag junction
--  table already stores the decomposed per-bit membership at the Industry
--  level; this column is the facility's own copy of that same bitmask value,
--  not a duplicate decomposition.
--
--  FK / MODULE BOUNDARY NOTE: IndustryName is a real FK into
--  [SafetyTaxonomy].[Industry].[Name] (UQ_Industry_Name) -- both schemas live
--  in the same [PlantSafety.CoreDB] solution database (database-per-solution
--  is honored; this is an intra-solution, cross-schema reference, not a
--  cross-database/cross-module-boundary violation), and every FacilityProfile
--  is built from exactly one canonical IndustryRegistry entry.
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FacilityProfile' AND schema_id = SCHEMA_ID('Facilities'))
BEGIN
    CREATE TABLE [Facilities].[FacilityProfile]
    (
        [FacilityId]     UNIQUEIDENTIFIER   NOT NULL,
        [Name]           NVARCHAR(200)      NOT NULL,
        [IndustryName]   NVARCHAR(200)      NOT NULL,
        [Location]       NVARCHAR(400)      NOT NULL,
        [Flags]          BIGINT             NOT NULL,

        [CreatedUtc]     DATETIME2(3)       NOT NULL CONSTRAINT DF_FacilityProfile_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]      NVARCHAR(128)      NOT NULL CONSTRAINT DF_FacilityProfile_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]     DATETIME2(3)       NULL,
        [UpdatedBy]      NVARCHAR(128)      NULL,
        [IsDeleted]      BIT                NOT NULL CONSTRAINT DF_FacilityProfile_IsDeleted DEFAULT (0),

        CONSTRAINT PK_FacilityProfile PRIMARY KEY CLUSTERED ([FacilityId]),
        CONSTRAINT FK_FacilityProfile_Industry FOREIGN KEY ([IndustryName])
            REFERENCES [SafetyTaxonomy].[Industry] ([Name])
    );
END
GO

------------------------------------------------------------
-- Indexes
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacilityProfile_IndustryName' AND object_id = OBJECT_ID('Facilities.FacilityProfile'))
    CREATE NONCLUSTERED INDEX IX_FacilityProfile_IndustryName
        ON [Facilities].[FacilityProfile] ([IndustryName]);
GO
