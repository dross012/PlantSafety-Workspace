-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement (Features-tree Incident aggregate)
--  Layer:          Infrastructure – SQL Scripts
--  File:           Incident.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team (Claude Code procedure-architect-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the Incident aggregate write path (IIncidentRepository /
--                  IncidentDapperRepository under Infrastructure\Features\SafetyEvents\
--                  IncidentManagement). Named [SafetyEvents].[Incident] -- distinct from the
--                  unrelated [SafetyEvents].[IncidentManagementItems] read-model table, which
--                  backs the separate IIncidentManagementRepository in this same module (see
--                  that table's header comment for the naming-collision rationale). Column set
--                  mirrors PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement
--                  .IncidentReadModel (the Dapper result-set contract) plus standard
--                  Created/soft-delete audit columns, following the AccidentManagement.Accident
--                  table's conventions (tenant-scoped, IsDeleted soft-delete flag,
--                  CreatedAtUtc/CreatedBy audit columns). Concurrency is enforced via a native
--                  ROWVERSION column exposed to callers as the string ETag (see
--                  ProcessManagement.usp_Process_GetById for the same CONVERT(NVARCHAR(34), ...)
--                  ETag-projection pattern), rather than the NEWID()-token ConcurrencyToken
--                  pattern used by Accident/Risk -- IncidentDapperRepository.UpdateAsync/
--                  SoftDeleteAsync pass an opaque @ETag string and expect a stale value to
--                  result in zero rows affected (surfaced as ConcurrencyConflictException).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Incident' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[Incident]
    (
        IncidentId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Incident PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NOT NULL,
        Title                  NVARCHAR(200)    NOT NULL,
        Description            NVARCHAR(4000)   NOT NULL,
        IncidentOccurredAtUtc  DATETIME2(3)     NOT NULL,
        ReportedAtUtc          DATETIME2(3)     NOT NULL,
        Severity               NVARCHAR(30)     NOT NULL,
        Status                 NVARCHAR(30)     NOT NULL CONSTRAINT DF_Incident_Status DEFAULT (N'Reported'),
        Location               NVARCHAR(500)    NOT NULL,
        ReportedByWorkerId     UNIQUEIDENTIFIER NOT NULL,
        AssignedInvestigatorId UNIQUEIDENTIFIER NULL,
        ClosedAtUtc            DATETIME2(3)     NULL,
        DeletionReason         NVARCHAR(1000)   NULL,
        DeletedAtUtc           DATETIME2(3)     NULL,
        IsDeleted              BIT              NOT NULL CONSTRAINT DF_Incident_IsDeleted DEFAULT (0),
        LastModifiedByActorId  UNIQUEIDENTIFIER NOT NULL,
        LastModifiedAtUtc      DATETIME2(3)     NOT NULL,
        CreatedAtUtc           DATETIME2(3)     NOT NULL,
        CreatedBy              UNIQUEIDENTIFIER NOT NULL,
        RowVersion             ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Incident_TenantId_SiteId ON [SafetyEvents].[Incident] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_Incident_TenantId_Status ON [SafetyEvents].[Incident] (TenantId, Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_Incident_TenantId_Severity ON [SafetyEvents].[Incident] (TenantId, Severity, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_Incident_TenantId_OccurredAtUtc ON [SafetyEvents].[Incident] (TenantId, IncidentOccurredAtUtc, IsDeleted);
END
GO
