-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase4-Facilities
--  Layer:          Infrastructure - SQL Scripts
--  File:           02-table-facilitycomplianceprofile.sql
--  Created:        2026-08-08
--  Author:         Platform Team (schema-sentinel-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [Facilities].[FacilityComplianceProfile] --
--                  the SQL read-model cache of Shared.SafetyData.
--                  Abstractions.Models.Industries.Facilities.
--                  FacilityComplianceProfile (FacilityComplianceProfile.cs),
--                  a facility's compliance scorecard. Built by
--                  FacilityComplianceProfileBuilder.Build and embedded 1:1 in
--                  every FacilityProfile.ComplianceProfile. Upserted via
--                  22-usp-facilitycomplianceprofile-upsert.sql.
--
--  StrongId / PK / FK: [FacilityId] -- this table is a genuine 1:1 child of
--  [Facilities].[FacilityProfile] (every FacilityProfile carries exactly one
--  ComplianceProfile, never independently constructed), so FacilityId is
--  both this table's PK and its FK back to FacilityProfile -- there is no
--  separate surrogate key, matching the 1:1 embedding in the C# record.
--  22-usp-facilitycomplianceprofile-upsert.sql enforces the parent row must
--  already exist before upserting a compliance profile.
--
--  COLUMN SHAPE: only the scalar scorecard summary (Required/Completed/
--  MissingElementsCount, ComplianceScorePercent, RiskLevel) is persisted --
--  matches 22-usp-facilitycomplianceprofile-upsert.sql's own column list
--  exactly (this table's DDL is grounded in that already-authored proc).
--  RequiredElements/CompletedElements/MissingElements/PriorityActions (the
--  full ComplianceElement lists) and Notes are NOT persisted here: every
--  element list is deterministically recomputable on demand from
--  FacilityProfile.RequiredPrograms (itself a pure function of the
--  IndustryName binding, already cached on [Facilities].[FacilityProfile])
--  plus the caller-supplied completedElementCodes set -- persisting the
--  flattened list would be redundant, mutable-in-two-places state; only the
--  scalar scorecard summary is cached.
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FacilityComplianceProfile' AND schema_id = SCHEMA_ID('Facilities'))
BEGIN
    CREATE TABLE [Facilities].[FacilityComplianceProfile]
    (
        [FacilityId]                UNIQUEIDENTIFIER   NOT NULL,
        [RequiredElementsCount]     INT                NOT NULL,
        [CompletedElementsCount]    INT                NOT NULL,
        [MissingElementsCount]      INT                NOT NULL,
        [ComplianceScorePercent]    FLOAT              NOT NULL,
        [RiskLevel]                 NVARCHAR(20)       NOT NULL,

        [CreatedUtc]                DATETIME2(3)       NOT NULL CONSTRAINT DF_FacilityComplianceProfile_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                 NVARCHAR(128)      NOT NULL CONSTRAINT DF_FacilityComplianceProfile_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]                DATETIME2(3)       NULL,
        [UpdatedBy]                 NVARCHAR(128)      NULL,
        [IsDeleted]                 BIT                NOT NULL CONSTRAINT DF_FacilityComplianceProfile_IsDeleted DEFAULT (0),

        CONSTRAINT PK_FacilityComplianceProfile PRIMARY KEY CLUSTERED ([FacilityId]),
        CONSTRAINT FK_FacilityComplianceProfile_FacilityProfile FOREIGN KEY ([FacilityId])
            REFERENCES [Facilities].[FacilityProfile] ([FacilityId]),
        CONSTRAINT CK_FacilityComplianceProfile_RiskLevel CHECK ([RiskLevel] IN (N'Low', N'Moderate', N'High'))
    );
END
GO
