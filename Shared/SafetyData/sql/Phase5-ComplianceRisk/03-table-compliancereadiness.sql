-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase5-ComplianceRisk
--  Layer:          Infrastructure - SQL Scripts
--  File:           03-table-compliancereadiness.sql
--  Created:        2026-08-08
--  Author:         Platform Team (schema-sentinel-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [ComplianceRisk].[ComplianceReadiness] --
--                  the SQL read-model cache of Shared.SafetyData.
--                  Abstractions.Models.Industries.ComplianceRisk.
--                  ComplianceReadiness (ComplianceReadiness.cs), built by
--                  ComplianceReadinessBuilder.Build(FacilityProfile): the
--                  facility's audit/training/permit/SOP readiness booleans
--                  plus an overall readiness percentage.
--
--  StrongId / PK / FK: composite (FacilityId, ComputedAtUtc) -- append-only
--  time series, mirroring 01-table-compliancescore.sql's pattern (see that
--  file's header and 00-create-schema.sql's SHAPE ASSUMPTION note).
--  FacilityId alone remains the FK back to [Facilities].[FacilityProfile].
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComplianceReadiness' AND schema_id = SCHEMA_ID('ComplianceRisk'))
BEGIN
    CREATE TABLE [ComplianceRisk].[ComplianceReadiness]
    (
        [FacilityId]                UNIQUEIDENTIFIER   NOT NULL,
        [ComputedAtUtc]             DATETIME2(3)       NOT NULL,
        [AuditReady]                BIT                NOT NULL,
        [TrainingReady]             BIT                NOT NULL,
        [PermitReady]               BIT                NOT NULL,
        [SopReady]                  BIT                NOT NULL,
        [OverallReadinessPercent]   FLOAT              NOT NULL,

        [CreatedUtc]                DATETIME2(3)       NOT NULL CONSTRAINT DF_ComplianceReadiness_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                 NVARCHAR(128)      NOT NULL CONSTRAINT DF_ComplianceReadiness_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]                DATETIME2(3)       NULL,
        [UpdatedBy]                 NVARCHAR(128)      NULL,
        [IsDeleted]                 BIT                NOT NULL CONSTRAINT DF_ComplianceReadiness_IsDeleted DEFAULT (0),

        CONSTRAINT PK_ComplianceReadiness PRIMARY KEY CLUSTERED ([FacilityId], [ComputedAtUtc]),
        CONSTRAINT FK_ComplianceReadiness_FacilityProfile FOREIGN KEY ([FacilityId])
            REFERENCES [Facilities].[FacilityProfile] ([FacilityId])
    );
END
GO
