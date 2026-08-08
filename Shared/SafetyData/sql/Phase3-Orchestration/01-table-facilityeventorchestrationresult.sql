-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase3-Orchestration
--  Layer:          Infrastructure - SQL Scripts
--  File:           01-table-facilityeventorchestrationresult.sql
--  Created:        2026-08-08
--  Author:         Platform Team (schema-sentinel-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [Orchestration].[FacilityEventOrchestrationResult]
--                  -- the persisted mirror of Shared.SafetyData.Abstractions.
--                  Models.Industries.Orchestration.FacilityEventOrchestrationResult
--                  (FacilityEventOrchestrationResult.cs), one row per real
--                  runtime event interpreted by EventOrchestrationService and
--                  recorded via InMemoryFacilityEventOrchestrationResultStore.
--                  Record(Guid eventId, FacilityEventOrchestrationResult
--                  result) (Reporting.Infrastructure\EventBus\ReadModels\
--                  InMemoryFacilityEventOrchestrationResultStore.cs).
--
--  StrongId / PK: [EventId] (UNIQUEIDENTIFIER) -- the C# store's own
--  idempotency key (the Guid eventId parameter passed alongside the result,
--  not itself a field on the FacilityEventOrchestrationResult record). This
--  is a genuinely raw, recorded fact (append-only event history): "a real
--  event occurred, and this is what it computed" -- never mutated after
--  insert, matching the C# store's idempotent-no-op-on-duplicate-EventId
--  semantics (see 20-usp-facilityeventorchestrationresult-record.sql).
--
--  COLUMN SHAPE: ComplianceScore/RiskScore fields are flattened onto this
--  table with a Compliance*/Risk* prefix (this table's own FacilityId
--  already carries ComplianceScore.FacilityId/RiskScore.FacilityId's value,
--  so that column is not duplicated). ComplianceReadiness/RiskReadiness/
--  DashboardAlerts are deliberately NOT persisted here -- they are
--  reproducible, deterministic recomputations from the same FacilityProfile
--  used to build ComplianceScore/RiskScore (see [ComplianceRisk] /
--  [Facilities] schemas), not additional raw facts about the event itself.
--
--  CANONICAL METADATA NOTE: unlike [SafetyTaxonomy] (static seeded reference
--  data with no per-row audit trail), this table is mutable, tenant-scoped,
--  event-sourced operational data, so it carries the full canonical
--  metadata set (CreatedUtc/CreatedBy/UpdatedUtc/UpdatedBy/IsDeleted) per
--  the Handbook. In practice rows are insert-only (UpdatedUtc/UpdatedBy stay
--  NULL, IsDeleted stays 0) because the owning C# store never updates or
--  deletes a recorded result -- the columns exist for governance
--  completeness/soft-delete-readiness, not because this table is expected to
--  be mutated. CreatedBy/IsDeleted carry DEFAULT constraints so that
--  20-usp-facilityeventorchestrationresult-record.sql's INSERT column list
--  (which supplies CreatedUtc explicitly but omits CreatedBy/UpdatedUtc/
--  UpdatedBy/IsDeleted) still succeeds.
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FacilityEventOrchestrationResult' AND schema_id = SCHEMA_ID('Orchestration'))
BEGIN
    CREATE TABLE [Orchestration].[FacilityEventOrchestrationResult]
    (
        [EventId]                             UNIQUEIDENTIFIER   NOT NULL,
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

        [CreatedUtc]                           DATETIME2(3)       NOT NULL CONSTRAINT DF_FacilityEventOrchestrationResult_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                            NVARCHAR(128)      NOT NULL CONSTRAINT DF_FacilityEventOrchestrationResult_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]                           DATETIME2(3)       NULL,
        [UpdatedBy]                            NVARCHAR(128)      NULL,
        [IsDeleted]                            BIT                NOT NULL CONSTRAINT DF_FacilityEventOrchestrationResult_IsDeleted DEFAULT (0),

        CONSTRAINT PK_FacilityEventOrchestrationResult PRIMARY KEY CLUSTERED ([EventId]),
        CONSTRAINT CK_FacilityEventOrchestrationResult_Category CHECK ([Category] IN (N'Created', N'Archived', N'Other')),
        CONSTRAINT CK_FacilityEventOrchestrationResult_ComplianceTier CHECK ([ComplianceTier] IN (N'NonCompliant', N'PartiallyCompliant', N'SubstantiallyCompliant', N'FullyCompliant')),
        CONSTRAINT CK_FacilityEventOrchestrationResult_RiskHazardLevel CHECK ([RiskHazardLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_FacilityEventOrchestrationResult_RiskProgramLevel CHECK ([RiskProgramLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_FacilityEventOrchestrationResult_RiskRegulatorLevel CHECK ([RiskRegulatorLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_FacilityEventOrchestrationResult_RiskEventLevel CHECK ([RiskEventLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_FacilityEventOrchestrationResult_RiskOverallLevel CHECK ([RiskOverallLevel] IN (N'Low', N'Medium', N'High', N'Critical'))
    );
END
GO
