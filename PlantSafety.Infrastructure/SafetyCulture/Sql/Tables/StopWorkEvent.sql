-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyCulture.StopWorkEvent
--  Layer:          Infrastructure - SQL Scripts
--  File:           StopWorkEvent.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the StopWorkEvent aggregate root backing
--                  IStopWorkEventRepository. InitiatedByActor is stored as two flat columns
--                  (InitiatedByActorKind, InitiatedByUserId) mirroring the PerformedByActor
--                  value type's Kind/UserId? shape. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StopWorkEvent' AND schema_id = SCHEMA_ID('SafetyCulture'))
BEGIN
    CREATE TABLE [SafetyCulture].[StopWorkEvent] (
        [StopWorkEventId]              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]                     UNIQUEIDENTIFIER NOT NULL,
        [SiteId]                       UNIQUEIDENTIFIER NULL,
        [Status]                       NVARCHAR(20)     NOT NULL CONSTRAINT DF_StopWorkEvent_Status DEFAULT ('Created'),
        [WorkArea]                     NVARCHAR(200)    NOT NULL,
        [HazardDescription]            NVARCHAR(2000)   NOT NULL,
        [InitiatedByActorKind]         NVARCHAR(20)     NOT NULL,
        [InitiatedByUserId]            UNIQUEIDENTIFIER NULL,
        [ResolutionSummary]            NVARCHAR(2000)   NULL,
        [WasCelebrated]                BIT              NOT NULL CONSTRAINT DF_StopWorkEvent_WasCelebrated DEFAULT (0),
        [RelatedPermitId]              UNIQUEIDENTIFIER NULL,
        [RelatedChecklistCompletionId] UNIQUEIDENTIFIER NULL,
        [InvestigationFindings]        NVARCHAR(2000)   NULL,
        [InvestigatedAtUtc]            DATETIME2(3)     NULL,
        [ResolvedAtUtc]                DATETIME2(3)     NULL,
        [CreatedAtUtc]                 DATETIME2(3)     NOT NULL,
        [CreatedBy]                    UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted]                    BIT              NOT NULL CONSTRAINT DF_StopWorkEvent_IsDeleted DEFAULT (0),
        [LastModifiedAtUtc]            DATETIME2(3)     NOT NULL CONSTRAINT DF_StopWorkEvent_LastModified DEFAULT (SYSUTCDATETIME()),
        [RowVersion]                   ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_StopWorkEvent_TenantId_SiteId]
        ON [SafetyCulture].[StopWorkEvent] ([TenantId], [SiteId])
        INCLUDE ([Status], [WorkArea]);
END
GO
