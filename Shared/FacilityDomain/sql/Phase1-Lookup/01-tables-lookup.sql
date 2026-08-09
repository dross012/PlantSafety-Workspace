-- ============================================================================
--  Project:        Shared.FacilityDomain
--  Module:         Phase1-Lookup
--  Layer:          Infrastructure - SQL Scripts
--  File:           01-tables-lookup.sql
--  Created:        2026-08-09
--  Author:         Platform Team (schema-sentinel-agent)
--  SchemaVersion:  2027.1
--  Description:    Table DDL for the three canonical lookup tables:
--                  [Lookup].[SeverityLevel], [Lookup].[HazardType],
--                  [Lookup].[RegulatoryJurisdiction]. Static, additive-only
--                  reference data (mirrors [SafetyTaxonomy]'s precedent for
--                  static seeded reference tables) -- Code is the natural key
--                  and StrongId every other table FKs against; OrdinalValue
--                  preserves the real C# enum's underlying int so ordering
--                  (e.g. SeverityLevel's "least to most severe") is
--                  reproducible in SQL without re-deriving it.
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

------------------------------------------------------------
-- Lookup.SeverityLevel (Shared.FacilityDomain.Abstractions.Severity.SeverityLevel)
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SeverityLevel' AND schema_id = SCHEMA_ID('Lookup'))
BEGIN
    CREATE TABLE [Lookup].[SeverityLevel]
    (
        [Code]          NVARCHAR(30)   NOT NULL,
        [OrdinalValue]  INT            NOT NULL,
        [Description]   NVARCHAR(400)  NOT NULL,

        [CreatedUtc]    DATETIME2(3)   NOT NULL CONSTRAINT DF_SeverityLevel_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]     NVARCHAR(128)  NOT NULL CONSTRAINT DF_SeverityLevel_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]    DATETIME2(3)   NULL,
        [UpdatedBy]     NVARCHAR(128)  NULL,
        [IsDeleted]     BIT            NOT NULL CONSTRAINT DF_SeverityLevel_IsDeleted DEFAULT (0),

        CONSTRAINT PK_SeverityLevel PRIMARY KEY CLUSTERED ([Code]),
        CONSTRAINT UQ_SeverityLevel_OrdinalValue UNIQUE ([OrdinalValue])
    );
END
GO

------------------------------------------------------------
-- Lookup.HazardType (Shared.FacilityDomain.Abstractions.Hazards.HazardType)
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HazardType' AND schema_id = SCHEMA_ID('Lookup'))
BEGIN
    CREATE TABLE [Lookup].[HazardType]
    (
        [Code]          NVARCHAR(30)   NOT NULL,
        [OrdinalValue]  INT            NOT NULL,
        [Description]   NVARCHAR(400)  NOT NULL,

        [CreatedUtc]    DATETIME2(3)   NOT NULL CONSTRAINT DF_HazardType_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]     NVARCHAR(128)  NOT NULL CONSTRAINT DF_HazardType_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]    DATETIME2(3)   NULL,
        [UpdatedBy]     NVARCHAR(128)  NULL,
        [IsDeleted]     BIT            NOT NULL CONSTRAINT DF_HazardType_IsDeleted DEFAULT (0),

        CONSTRAINT PK_HazardType PRIMARY KEY CLUSTERED ([Code]),
        CONSTRAINT UQ_HazardType_OrdinalValue UNIQUE ([OrdinalValue])
    );
END
GO

------------------------------------------------------------
-- Lookup.RegulatoryJurisdiction (Shared.FacilityDomain.Abstractions.Regulatory.RegulatoryJurisdiction)
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RegulatoryJurisdiction' AND schema_id = SCHEMA_ID('Lookup'))
BEGIN
    CREATE TABLE [Lookup].[RegulatoryJurisdiction]
    (
        [Code]          NVARCHAR(30)   NOT NULL,
        [OrdinalValue]  INT            NOT NULL,
        [Description]   NVARCHAR(400)  NOT NULL,

        [CreatedUtc]    DATETIME2(3)   NOT NULL CONSTRAINT DF_RegulatoryJurisdiction_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]     NVARCHAR(128)  NOT NULL CONSTRAINT DF_RegulatoryJurisdiction_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]    DATETIME2(3)   NULL,
        [UpdatedBy]     NVARCHAR(128)  NULL,
        [IsDeleted]     BIT            NOT NULL CONSTRAINT DF_RegulatoryJurisdiction_IsDeleted DEFAULT (0),

        CONSTRAINT PK_RegulatoryJurisdiction PRIMARY KEY CLUSTERED ([Code]),
        CONSTRAINT UQ_RegulatoryJurisdiction_OrdinalValue UNIQUE ([OrdinalValue])
    );
END
GO
