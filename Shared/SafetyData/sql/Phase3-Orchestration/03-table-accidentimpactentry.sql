-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase3-Orchestration
--  Layer:          Infrastructure - SQL Scripts
--  File:           03-table-accidentimpactentry.sql
--  Created:        2026-08-08
--  Author:         Platform Team (schema-sentinel-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [Orchestration].[AccidentImpactEntry] -- the
--                  persisted mirror of Reporting.Infrastructure.EventBus.
--                  ReadModels.AccidentImpactEntry (AccidentImpactEntry.cs):
--                  a single AccidentManagement event's recomputed facility
--                  impact, captured alongside the real AccidentId. Recorded
--                  via InMemoryAccidentImpactStore.Record(Guid eventId,
--                  AccidentImpactEntry entry).
--
--  COLUMN SHAPE: AccidentImpactEntry wraps the full
--  FacilityEventOrchestrationResult (its own Result property) alongside
--  AccidentId, so this table's columns are the same flattened
--  Compliance*/Risk* shape as [Orchestration].[FacilityEventOrchestrationResult]
--  (01-table-facilityeventorchestrationresult.sql) plus AccidentId -- a
--  deliberately denormalized, self-contained "additive event-history
--  variant" (see 00-create-schema.sql's CONCURRENT WORK NOTE), not a FK-only
--  join table, since AccidentManagement/RiskManagement events are each their
--  own independently-queryable stream (GetByAccidentId) and must remain
--  queryable even if the base FacilityEventOrchestrationResult row were ever
--  purged independently.
--
--  StrongId / PK: [EventId] (UNIQUEIDENTIFIER) -- same idempotency key
--  rationale as FacilityEventOrchestrationResult (see that file's header).
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AccidentImpactEntry' AND schema_id = SCHEMA_ID('Orchestration'))
BEGIN
    CREATE TABLE [Orchestration].[AccidentImpactEntry]
    (
        [EventId]                             UNIQUEIDENTIFIER   NOT NULL,
        [AccidentId]                           UNIQUEIDENTIFIER   NOT NULL,
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

        [CreatedUtc]                           DATETIME2(3)       NOT NULL CONSTRAINT DF_AccidentImpactEntry_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                            NVARCHAR(128)      NOT NULL CONSTRAINT DF_AccidentImpactEntry_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]                           DATETIME2(3)       NULL,
        [UpdatedBy]                            NVARCHAR(128)      NULL,
        [IsDeleted]                            BIT                NOT NULL CONSTRAINT DF_AccidentImpactEntry_IsDeleted DEFAULT (0),

        CONSTRAINT PK_AccidentImpactEntry PRIMARY KEY CLUSTERED ([EventId]),
        CONSTRAINT CK_AccidentImpactEntry_Category CHECK ([Category] IN (N'Created', N'Archived', N'Other')),
        CONSTRAINT CK_AccidentImpactEntry_ComplianceTier CHECK ([ComplianceTier] IN (N'NonCompliant', N'PartiallyCompliant', N'SubstantiallyCompliant', N'FullyCompliant')),
        CONSTRAINT CK_AccidentImpactEntry_RiskHazardLevel CHECK ([RiskHazardLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_AccidentImpactEntry_RiskProgramLevel CHECK ([RiskProgramLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_AccidentImpactEntry_RiskRegulatorLevel CHECK ([RiskRegulatorLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_AccidentImpactEntry_RiskEventLevel CHECK ([RiskEventLevel] IN (N'Low', N'Medium', N'High', N'Critical')),
        CONSTRAINT CK_AccidentImpactEntry_RiskOverallLevel CHECK ([RiskOverallLevel] IN (N'Low', N'Medium', N'High', N'Critical'))
    );
END
GO
