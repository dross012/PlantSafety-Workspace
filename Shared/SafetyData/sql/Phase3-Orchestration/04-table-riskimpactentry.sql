-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase3-Orchestration
--  Layer:          Infrastructure - SQL Scripts
--  File:           04-table-riskimpactentry.sql
--  Created:        2026-08-08
--  Author:         Platform Team (schema-sentinel-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [Orchestration].[RiskImpactEntry] -- the
--                  persisted mirror of Reporting.Infrastructure.EventBus.
--                  ReadModels.RiskImpactEntry (RiskImpactEntry.cs): a single
--                  RiskManagement event's recomputed facility impact,
--                  captured alongside the real RiskId. Recorded via
--                  InMemoryRiskImpactStore.Record(Guid eventId,
--                  RiskImpactEntry entry). Mirrors
--                  [Orchestration].[AccidentImpactEntry]'s shape exactly
--                  (see 03-table-accidentimpactentry.sql's header for the
--                  full column-shape/denormalization rationale), with RiskId
--                  in place of AccidentId.
--
--  StrongId / PK: [EventId] (UNIQUEIDENTIFIER) -- same idempotency key
--  rationale as FacilityEventOrchestrationResult
--  (01-table-facilityeventorchestrationresult.sql's header).
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RiskImpactEntry' AND schema_id = SCHEMA_ID('Orchestration'))
BEGIN
    CREATE TABLE [Orchestration].[RiskImpactEntry]
    (
        [EventId]                             UNIQUEIDENTIFIER   NOT NULL,
        [RiskId]                               UNIQUEIDENTIFIER   NOT NULL,
        [TenantId]                             NVARCHAR(100)      NOT NULL,
        [FacilityId]                           UNIQUEIDENTIFIER   NOT NULL,
        [SourceModule]                         NVARCHAR(200)      NOT NULL,
        [EventType]                            NVARCHAR(200)      NOT NULL,
        [OccurredAtUtc]                        DATETIME2(3)       NOT NULL,

        [ComplianceRequiredElementsCount]      INT                NOT NULL,
        [ComplianceCompletedElementsCount]     INT                NOT NULL,
        [ComplianceMissingElementsCount]       INT                NOT NULL,
        [CompliancePercentComplete]            FLOAT              NOT NULL,
        [ComplianceTier]                       NVARCHAR(30)       NOT NULL,
        [ComplianceNotes]                      NVARCHAR(400)      NOT NULL,

        [RiskHazardLevel]                      NVARCHAR(20)       NOT NULL,
        [RiskProgramLevel]                     NVARCHAR(20)       NOT NULL,
        [RiskRegulatorLevel]                   NVARCHAR(20)       NOT NULL,
        [RiskEventLevel]                       NVARCHAR(20)       NOT NULL,
        [RiskOverallLevel]                     NVARCHAR(20)       NOT NULL,

        [Category]                             NVARCHAR(20)       NOT NULL,
        [RequiresImmediateAttention]           BIT                NOT NULL,
        [Notification]                         NVARCHAR(1000)     NOT NULL,

        [CreatedUtc]                           DATETIME2(3)       NOT NULL CONSTRAINT DF_RiskImpactEntry_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                            NVARCHAR(128)      NOT NULL CONSTRAINT DF_RiskImpactEntry_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]                           DATETIME2(3)       NULL,
        [UpdatedBy]                            NVARCHAR(128)      NULL,
        [IsDeleted]                            BIT                NOT NULL CONSTRAINT DF_RiskImpactEntry_IsDeleted DEFAULT (0),

        CONSTRAINT PK_RiskImpactEntry PRIMARY KEY CLUSTERED ([EventId]),
        CONSTRAINT CK_RiskImpactEntry_Category CHECK ([Category] IN (N'Created', N'Archived', N'Other')),
        CONSTRAINT CK_RiskImpactEntry_ComplianceTier CHECK ([ComplianceTier] IN (N'NonCompliant', N'PartiallyCompliant', N'SubstantiallyCompliant', N'FullyCompliant')),
        CONSTRAINT CK_RiskImpactEntry_RiskHazardLevel CHECK ([RiskHazardLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_RiskImpactEntry_RiskProgramLevel CHECK ([RiskProgramLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_RiskImpactEntry_RiskRegulatorLevel CHECK ([RiskRegulatorLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_RiskImpactEntry_RiskEventLevel CHECK ([RiskEventLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_RiskImpactEntry_RiskOverallLevel CHECK ([RiskOverallLevel] IN (N'Low', N'Medium', N'High', N'Critical'))
    );
END
GO
