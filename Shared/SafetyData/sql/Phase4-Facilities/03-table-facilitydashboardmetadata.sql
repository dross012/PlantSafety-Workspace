-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase4-Facilities
--  Layer:          Infrastructure - SQL Scripts
--  File:           03-table-facilitydashboardmetadata.sql
--  Created:        2026-08-08
--  Author:         Platform Team (schema-sentinel-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [Facilities].[FacilityDashboardMetadata] --
--                  the SQL read-model cache of Shared.SafetyData.
--                  Abstractions.Models.Industries.Facilities.
--                  FacilityDashboardMetadata (FacilityDashboardMetadata.cs):
--                  a facility's KPIs, required widgets, required charts, and
--                  required alerts. Built by
--                  FacilityDashboardMetadataBuilder.Build(FacilityProfile).
--
--  NAMING / COLLISION NOTE: this is a DIFFERENT table from PlantSafety's
--  existing [Compliance]/[Dashboards]-adjacent objects (this schema is
--  [Facilities], not [Compliance] or [Dashboards]) -- see
--  00-create-schema.sql's header for the full [Facilities] schema rationale.
--
--  NOT explicitly listed in Phase4-Facilities\00-create-schema.sql's own
--  description (which names only FacilityProfile/FacilityComplianceProfile)
--  -- added per the task's explicit instruction to build a table for
--  FacilityDashboardMetadata as well; additive, does not conflict with
--  either of the other two tables' DDL or FKs.
--
--  StrongId / PK / FK: [FacilityId] -- 1:1 with [Facilities].[FacilityProfile]
--  (FacilityDashboardMetadataBuilder.Build takes a single FacilityProfile and
--  returns exactly one FacilityDashboardMetadata), so FacilityId is both PK
--  and FK, mirroring 02-table-facilitycomplianceprofile.sql's pattern.
--
--  COLUMN SHAPE: KeyIndicators/RequiredWidgets/RequiredCharts/RequiredAlerts
--  are all IReadOnlyList<T> in the C# record -- persisted as JSON array
--  snapshot columns for the same reason documented in
--  01-table-facilityprofile.sql's COLUMN SHAPE note.
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FacilityDashboardMetadata' AND schema_id = SCHEMA_ID('Facilities'))
BEGIN
    CREATE TABLE [Facilities].[FacilityDashboardMetadata]
    (
        [FacilityId]             UNIQUEIDENTIFIER   NOT NULL,

        [KeyIndicatorsJson]      NVARCHAR(MAX)      NOT NULL,
        [RequiredWidgetsJson]    NVARCHAR(MAX)      NOT NULL,
        [RequiredChartsJson]     NVARCHAR(MAX)      NOT NULL,
        [RequiredAlertsJson]     NVARCHAR(MAX)      NOT NULL,

        [CreatedUtc]             DATETIME2(3)       NOT NULL CONSTRAINT DF_FacilityDashboardMetadata_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]              NVARCHAR(128)      NOT NULL CONSTRAINT DF_FacilityDashboardMetadata_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]             DATETIME2(3)       NULL,
        [UpdatedBy]              NVARCHAR(128)      NULL,
        [IsDeleted]              BIT                NOT NULL CONSTRAINT DF_FacilityDashboardMetadata_IsDeleted DEFAULT (0),

        CONSTRAINT PK_FacilityDashboardMetadata PRIMARY KEY CLUSTERED ([FacilityId]),
        CONSTRAINT FK_FacilityDashboardMetadata_FacilityProfile FOREIGN KEY ([FacilityId])
            REFERENCES [Facilities].[FacilityProfile] ([FacilityId]),
        CONSTRAINT CK_FacilityDashboardMetadata_KeyIndicatorsJson CHECK (ISJSON([KeyIndicatorsJson]) = 1),
        CONSTRAINT CK_FacilityDashboardMetadata_RequiredWidgetsJson CHECK (ISJSON([RequiredWidgetsJson]) = 1),
        CONSTRAINT CK_FacilityDashboardMetadata_RequiredChartsJson CHECK (ISJSON([RequiredChartsJson]) = 1),
        CONSTRAINT CK_FacilityDashboardMetadata_RequiredAlertsJson CHECK (ISJSON([RequiredAlertsJson]) = 1)
    );
END
GO
