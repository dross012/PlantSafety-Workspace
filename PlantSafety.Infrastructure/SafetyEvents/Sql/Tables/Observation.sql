-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.Observation
--  Layer:          Infrastructure – SQL Scripts
--  File:           Observation.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the [SafetyEvents].[Observation] aggregate, backing
--                  IObservationRepository / ObservationDapperRepository (PlantSafety.Infrastructure.
--                  Features.SafetyEvents.Observation.Repositories). This is DISTINCT from
--                  [SafetyEvents].[ObservationItems]/[ObservationHistoryEntry] and the legacy
--                  sp_Observation_* procs, which back an unrelated, separately-built read model
--                  with no Title/Description/ObservationType columns. Mirrors the same
--                  dual-implementation situation discovered for NearMiss/NearMisses (see
--                  NearMiss.sql): this Features-namespaced aggregate (CreateObservationCommand,
--                  created 2026-06-17) never got SQL backing.
--                  Column list mirrors ObservationReadModel and the StoredProcedureBuilder Add*
--                  calls in ObservationDapperRepository exactly. Status has no setter anywhere in
--                  IObservationRepository (Create/Update never pass it), so it is a fixed
--                  'Recorded' default written once at Create and never mutated by this feature.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Observation' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[Observation] (
        ObservationId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Observation PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteId                  UNIQUEIDENTIFIER NOT NULL,
        Title                   NVARCHAR(200)    NOT NULL,
        Description              NVARCHAR(4000)   NOT NULL,
        ObservationType          NVARCHAR(30)     NOT NULL,
        ObservedAtUtc            DATETIME2(3)     NOT NULL,
        ObservedByWorkerId       UNIQUEIDENTIFIER NOT NULL,
        Status                   NVARCHAR(30)     NOT NULL CONSTRAINT DF_Observation_Status DEFAULT (N'Recorded'),
        CreatedAtUtc             DATETIME2(3)     NOT NULL,
        CreatedBy                UNIQUEIDENTIFIER NOT NULL,
        LastModifiedByActorId    UNIQUEIDENTIFIER NULL,
        LastModifiedAtUtc        DATETIME2(3)     NOT NULL,
        RowVersion               ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Observation_TenantId_ObservedAtUtc ON [SafetyEvents].[Observation] (TenantId, ObservedAtUtc);
    CREATE NONCLUSTERED INDEX IX_Observation_TenantId_SiteId ON [SafetyEvents].[Observation] (TenantId, SiteId);
END
GO
