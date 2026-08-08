-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase5-ComplianceRisk
--  Layer:          Infrastructure - SQL Scripts
--  File:           01-table-compliancescore.sql
--  Created:        2026-08-08
--  Author:         Platform Team (schema-sentinel-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [ComplianceRisk].[ComplianceScore] -- the
--                  SQL read-model cache of Shared.SafetyData.Abstractions.
--                  Models.Industries.ComplianceRisk.ComplianceScore
--                  (ComplianceScore.cs), built by
--                  ComplianceScoreBuilder.Build(FacilityProfile): the
--                  facility's Required/Completed/Missing element counts
--                  (read directly off FacilityComplianceProfile, never
--                  independently recomputed) re-framed into a
--                  ComplianceTier with explanatory Notes.
--
--  StrongId / PK / FK: composite (FacilityId, ComputedAtUtc) -- per
--  00-create-schema.sql's SHAPE ASSUMPTION note, this table is an
--  append-only time series (one row per recomputation of a facility's
--  ComplianceScore), not a single upserted current-state row, matching
--  20-usp-compliancescore-record.sql's idempotent-on-(FacilityId,
--  ComputedAtUtc) insert. FacilityId alone remains the FK back to
--  [Facilities].[FacilityProfile].
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComplianceScore' AND schema_id = SCHEMA_ID('ComplianceRisk'))
BEGIN
    CREATE TABLE [ComplianceRisk].[ComplianceScore]
    (
        [FacilityId]                 UNIQUEIDENTIFIER   NOT NULL,
        [ComputedAtUtc]               DATETIME2(3)       NOT NULL,
        [RequiredElementsCount]      INT                NOT NULL,
        [CompletedElementsCount]     INT                NOT NULL,
        [MissingElementsCount]       INT                NOT NULL,
        [PercentComplete]            FLOAT              NOT NULL,
        [ComplianceTier]             NVARCHAR(30)       NOT NULL,
        [Notes]                      NVARCHAR(400)      NOT NULL,

        [CreatedUtc]                 DATETIME2(3)       NOT NULL CONSTRAINT DF_ComplianceScore_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                  NVARCHAR(128)      NOT NULL CONSTRAINT DF_ComplianceScore_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]                 DATETIME2(3)       NULL,
        [UpdatedBy]                  NVARCHAR(128)      NULL,
        [IsDeleted]                  BIT                NOT NULL CONSTRAINT DF_ComplianceScore_IsDeleted DEFAULT (0),

        CONSTRAINT PK_ComplianceScore PRIMARY KEY CLUSTERED ([FacilityId], [ComputedAtUtc]),
        CONSTRAINT FK_ComplianceScore_FacilityProfile FOREIGN KEY ([FacilityId])
            REFERENCES [Facilities].[FacilityProfile] ([FacilityId]),
        CONSTRAINT CK_ComplianceScore_ComplianceTier CHECK ([ComplianceTier] IN (N'NonCompliant', N'PartiallyCompliant', N'SubstantiallyCompliant', N'FullyCompliant'))
    );
END
GO
