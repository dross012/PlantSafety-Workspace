-- ============================================================================
--  Project:        Shared.FacilityDomain -- Phase2-Facility
--  File:           01-table-facility.sql
--  Created:        2026-08-09
--  Author:         Platform Team (schema-sentinel-agent)
--  Description:    Table DDL for [FacilityDomain].[Facility] -- the durable
--                  mirror of FacilityAggregate/FacilityState's scalar
--                  identity fields (FacilityState.cs: FacilityId, Name,
--                  Industry.IndustryName/NaicsCode, Location.*,
--                  LifecycleStatus, CreatedAtUtc/UpdatedAtUtc). One row per
--                  FacilityId, upserted by FacilityService whenever the
--                  aggregate's state changes (UpdateFacilityMetadata,
--                  Activate/Shutdown/Reopen/DecommissionFacility).
--
--  StrongId / PK: [FacilityId] (UNIQUEIDENTIFIER), matching
--  Shared.FacilityDomain.Abstractions.ValueObjects.FacilityId's wrapped Guid.
--
--  COLUMN SHAPE: FacilityIndustry/FacilityLocation are flattened (value-
--  object columns), matching [Facilities].[FacilityProfile]'s own precedent
--  for embedding a caller-supplied value object as scalar columns rather
--  than a separate 1:1 table. HazardProfile/RegulatoryProfile/ProgramProfile
--  (each a replace-wholesale IReadOnlyList<T> "as of a point in time") and
--  Hierarchy are NOT embedded here -- each gets its own child table below /
--  in its own file, since they are collections, not scalars.
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Facility' AND schema_id = SCHEMA_ID('FacilityDomain'))
BEGIN
    CREATE TABLE [FacilityDomain].[Facility]
    (
        [FacilityId]         UNIQUEIDENTIFIER  NOT NULL,
        [Name]                NVARCHAR(200)     NOT NULL,
        [IndustryName]        NVARCHAR(200)     NOT NULL,
        [NaicsCode]           NVARCHAR(20)      NULL,
        [AddressLine1]        NVARCHAR(200)     NOT NULL,
        [City]                NVARCHAR(100)     NOT NULL,
        [StateOrProvince]     NVARCHAR(100)     NOT NULL,
        [PostalCode]          NVARCHAR(20)      NOT NULL,
        [CountryCode]         NVARCHAR(10)      NOT NULL,
        [LifecycleStatus]     NVARCHAR(30)      NOT NULL CONSTRAINT DF_Facility_LifecycleStatus DEFAULT (N'PendingActivation'),
        [CreatedAtUtc]        DATETIME2(3)      NOT NULL,
        [UpdatedAtUtc]        DATETIME2(3)      NOT NULL,

        [CreatedUtc]          DATETIME2(3)      NOT NULL CONSTRAINT DF_Facility_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]           NVARCHAR(128)     NOT NULL CONSTRAINT DF_Facility_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]          DATETIME2(3)      NULL,
        [UpdatedBy]           NVARCHAR(128)     NULL,
        [IsDeleted]           BIT               NOT NULL CONSTRAINT DF_Facility_IsDeleted DEFAULT (0),

        CONSTRAINT PK_Facility PRIMARY KEY CLUSTERED ([FacilityId]),
        CONSTRAINT CK_Facility_LifecycleStatus CHECK ([LifecycleStatus] IN (N'PendingActivation', N'Active', N'ShutDown', N'Decommissioned'))
    );
END
GO

------------------------------------------------------------
-- FacilityDomain.FacilityHazardProfileItem
-- (FacilityHazardProfile.ActiveHazardTypes -- replaced wholesale on
--  UpdateHazardProfile; modeled as a child table keyed by (FacilityId,
--  HazardTypeCode) rather than a JSON list, since HazardType is itself a
--  canonical FK'd lookup and per-hazard querying/reporting is expected.)
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FacilityHazardProfileItem' AND schema_id = SCHEMA_ID('FacilityDomain'))
BEGIN
    CREATE TABLE [FacilityDomain].[FacilityHazardProfileItem]
    (
        [FacilityId]       UNIQUEIDENTIFIER  NOT NULL,
        [HazardTypeCode]   NVARCHAR(30)      NOT NULL,
        [LastUpdatedUtc]   DATETIME2(3)      NOT NULL,

        CONSTRAINT PK_FacilityHazardProfileItem PRIMARY KEY CLUSTERED ([FacilityId], [HazardTypeCode]),
        CONSTRAINT FK_FacilityHazardProfileItem_Facility FOREIGN KEY ([FacilityId])
            REFERENCES [FacilityDomain].[Facility] ([FacilityId]),
        CONSTRAINT FK_FacilityHazardProfileItem_HazardType FOREIGN KEY ([HazardTypeCode])
            REFERENCES [Lookup].[HazardType] ([Code])
    );
END
GO

------------------------------------------------------------
-- FacilityDomain.FacilityRegulatoryProfileItem
-- (FacilityRegulatoryProfile.RegulatorCodes -- plain strings in the C#
--  record, not yet FK'd to [Lookup].[RegulatoryJurisdiction]; kept as a
--  free-text code column to avoid silently rejecting a caller-supplied
--  regulator code that isn't one of the six canonical jurisdictions, per
--  FacilityRegulatoryProfile.cs's own "kept as plain codes" independence
--  rationale.)
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FacilityRegulatoryProfileItem' AND schema_id = SCHEMA_ID('FacilityDomain'))
BEGIN
    CREATE TABLE [FacilityDomain].[FacilityRegulatoryProfileItem]
    (
        [FacilityId]       UNIQUEIDENTIFIER  NOT NULL,
        [RegulatorCode]    NVARCHAR(50)      NOT NULL,
        [LastUpdatedUtc]   DATETIME2(3)      NOT NULL,

        CONSTRAINT PK_FacilityRegulatoryProfileItem PRIMARY KEY CLUSTERED ([FacilityId], [RegulatorCode]),
        CONSTRAINT FK_FacilityRegulatoryProfileItem_Facility FOREIGN KEY ([FacilityId])
            REFERENCES [FacilityDomain].[Facility] ([FacilityId])
    );
END
GO

------------------------------------------------------------
-- FacilityDomain.FacilityProgramProfileItem
-- (FacilityProgramProfile.RequiredProgramCodes)
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FacilityProgramProfileItem' AND schema_id = SCHEMA_ID('FacilityDomain'))
BEGIN
    CREATE TABLE [FacilityDomain].[FacilityProgramProfileItem]
    (
        [FacilityId]       UNIQUEIDENTIFIER  NOT NULL,
        [ProgramCode]      NVARCHAR(50)      NOT NULL,
        [LastUpdatedUtc]   DATETIME2(3)      NOT NULL,

        CONSTRAINT PK_FacilityProgramProfileItem PRIMARY KEY CLUSTERED ([FacilityId], [ProgramCode]),
        CONSTRAINT FK_FacilityProgramProfileItem_Facility FOREIGN KEY ([FacilityId])
            REFERENCES [FacilityDomain].[Facility] ([FacilityId])
    );
END
GO
