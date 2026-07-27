-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.ContinuousImprovement.CAPA
--  Layer:          Infrastructure – SQL Scripts
--  File:           CorrectiveAction.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team (Claude Code procedure-architect-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the [ContinuousImprovement].[CorrectiveAction] aggregate
--                  (CAPA -- Corrective and Preventive Action), backing
--                  ICorrectiveActionRepository / CorrectiveActionDapperRepository
--                  (PlantSafety.Infrastructure.Features.ContinuousImprovement.CAPA.Repositories).
--                  This is a DISTINCT table from [ContinuousImprovement].[CAPA] (see CAPA.sql),
--                  which backs the unrelated ICAPARepository.GetActionsAsync reporting read-model
--                  (different columns: CapaId/SiteId/DueDateUtc/AssignedTo, no SourceEventId,
--                  Description, Priority, CompletedAtUtc, LastModifiedAtUtc, or ETag). It is also
--                  distinct from [AccidentManagement].[CorrectiveAction] (see
--                  AccidentManagement\Sql\Tables\CorrectiveAction.sql), which is a child entity of
--                  Accident and is unrelated to this CAPA feature. Column list mirrors
--                  CorrectiveActionReadModel (PlantSafety.Application.Features.
--                  ContinuousImprovement.Handlers.CAPA.ICorrectiveActionRepository) exactly, plus
--                  standard audit/soft-delete columns per the canonical FY2027 pattern.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ContinuousImprovement')
BEGIN
    EXEC('CREATE SCHEMA [ContinuousImprovement]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CorrectiveAction' AND schema_id = SCHEMA_ID('ContinuousImprovement'))
BEGIN
    CREATE TABLE [ContinuousImprovement].[CorrectiveAction] (
        CaId                   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SourceEventId          UNIQUEIDENTIFIER NOT NULL,
        SourceEventType        NVARCHAR(100)    NOT NULL,
        Title                  NVARCHAR(200)    NOT NULL,
        Description            NVARCHAR(4000)   NOT NULL,
        ActionType             NVARCHAR(50)     NOT NULL,
        Priority               NVARCHAR(50)     NOT NULL,
        AssignedToWorkerId     UNIQUEIDENTIFIER NOT NULL,
        DueAtUtc               DATETIME2(3)     NOT NULL,
        Status                 NVARCHAR(50)     NOT NULL CONSTRAINT DF_CorrectiveAction_Status DEFAULT (N'Open'),
        CompletedAtUtc         DATETIME2(3)     NULL,
        IsDeleted              BIT              NOT NULL CONSTRAINT DF_CorrectiveAction_IsDeleted DEFAULT (0),
        DeletedAtUtc           DATETIME2(3)     NULL,
        DeletionReason         NVARCHAR(400)    NULL,
        LastModifiedByActorId  UNIQUEIDENTIFIER NULL,
        LastModifiedAtUtc      DATETIME2(3)     NOT NULL CONSTRAINT DF_CorrectiveAction_LastModifiedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedAtUtc           DATETIME2(3)     NOT NULL CONSTRAINT DF_CorrectiveAction_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy              UNIQUEIDENTIFIER NULL,
        RowVersion             ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CorrectiveAction_TenantId_SourceEventId] ON [ContinuousImprovement].[CorrectiveAction] (TenantId, SourceEventId);
    CREATE NONCLUSTERED INDEX [IX_CorrectiveAction_TenantId_Status_DueAtUtc] ON [ContinuousImprovement].[CorrectiveAction] (TenantId, Status, DueAtUtc);
END
GO
