-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase5-ComplianceRisk
--  Layer:          Infrastructure - SQL Scripts
--  File:           02-table-riskscore.sql
--  Created:        2026-08-08
--  Author:         Platform Team (schema-sentinel-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [ComplianceRisk].[RiskScore] -- the SQL
--                  read-model cache of Shared.SafetyData.Abstractions.Models.
--                  Industries.ComplianceRisk.RiskScore (RiskScore.cs), built
--                  by RiskScoreBuilder.Build(FacilityProfile): the facility's
--                  multi-dimensional risk score (hazard/program/regulator/
--                  event dimensions, plus the overall -- max of the four --
--                  level).
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RiskScore' AND schema_id = SCHEMA_ID('ComplianceRisk'))
BEGIN
    CREATE TABLE [ComplianceRisk].[RiskScore]
    (
        [FacilityId]           UNIQUEIDENTIFIER   NOT NULL,
        [ComputedAtUtc]        DATETIME2(3)       NOT NULL,
        [HazardRiskLevel]      NVARCHAR(20)       NOT NULL,
        [ProgramRiskLevel]     NVARCHAR(20)       NOT NULL,
        [RegulatorRiskLevel]   NVARCHAR(20)       NOT NULL,
        [EventRiskLevel]       NVARCHAR(20)       NOT NULL,
        [OverallRiskLevel]     NVARCHAR(20)       NOT NULL,

        [CreatedUtc]           DATETIME2(3)       NOT NULL CONSTRAINT DF_RiskScore_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]            NVARCHAR(128)      NOT NULL CONSTRAINT DF_RiskScore_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]           DATETIME2(3)       NULL,
        [UpdatedBy]            NVARCHAR(128)      NULL,
        [IsDeleted]            BIT                NOT NULL CONSTRAINT DF_RiskScore_IsDeleted DEFAULT (0),

        CONSTRAINT PK_RiskScore PRIMARY KEY CLUSTERED ([FacilityId], [ComputedAtUtc]),
        CONSTRAINT FK_RiskScore_FacilityProfile FOREIGN KEY ([FacilityId])
            REFERENCES [Facilities].[FacilityProfile] ([FacilityId]),
        CONSTRAINT CK_RiskScore_HazardRiskLevel CHECK ([HazardRiskLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_RiskScore_ProgramRiskLevel CHECK ([ProgramRiskLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_RiskScore_RegulatorRiskLevel CHECK ([RegulatorRiskLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_RiskScore_EventRiskLevel CHECK ([EventRiskLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_RiskScore_OverallRiskLevel CHECK ([OverallRiskLevel] IN (N'Low', N'Medium', N'High', N'Critical'))
    );
END
GO
