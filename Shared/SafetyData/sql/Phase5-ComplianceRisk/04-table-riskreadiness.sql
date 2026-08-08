-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase5-ComplianceRisk
--  Layer:          Infrastructure - SQL Scripts
--  File:           04-table-riskreadiness.sql
--  Created:        2026-08-08
--  Author:         Platform Team (schema-sentinel-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [ComplianceRisk].[RiskReadiness] -- the SQL
--                  read-model cache of Shared.SafetyData.Abstractions.Models.
--                  Industries.ComplianceRisk.RiskReadiness
--                  (RiskReadiness.cs), built by
--                  RiskReadinessBuilder.Build(FacilityProfile): the
--                  facility's hazard-response/emergency-response/
--                  event-routing readiness booleans plus an overall
--                  readiness percentage.
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RiskReadiness' AND schema_id = SCHEMA_ID('ComplianceRisk'))
BEGIN
    CREATE TABLE [ComplianceRisk].[RiskReadiness]
    (
        [FacilityId]                 UNIQUEIDENTIFIER   NOT NULL,
        [ComputedAtUtc]              DATETIME2(3)       NOT NULL,
        [HazardResponseReady]        BIT                NOT NULL,
        [EmergencyResponseReady]     BIT                NOT NULL,
        [EventRoutingReady]          BIT                NOT NULL,
        [OverallReadinessPercent]    FLOAT              NOT NULL,

        [CreatedUtc]                 DATETIME2(3)       NOT NULL CONSTRAINT DF_RiskReadiness_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                  NVARCHAR(128)      NOT NULL CONSTRAINT DF_RiskReadiness_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]                 DATETIME2(3)       NULL,
        [UpdatedBy]                  NVARCHAR(128)      NULL,
        [IsDeleted]                  BIT                NOT NULL CONSTRAINT DF_RiskReadiness_IsDeleted DEFAULT (0),

        CONSTRAINT PK_RiskReadiness PRIMARY KEY CLUSTERED ([FacilityId], [ComputedAtUtc]),
        CONSTRAINT FK_RiskReadiness_FacilityProfile FOREIGN KEY ([FacilityId])
            REFERENCES [Facilities].[FacilityProfile] ([FacilityId])
    );
END
GO
