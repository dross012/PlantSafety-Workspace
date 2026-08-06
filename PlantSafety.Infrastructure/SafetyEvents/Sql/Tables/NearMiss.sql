-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.NearMisses
--  Layer:          Infrastructure – SQL Scripts
--  File:           NearMiss.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the [SafetyEvents].[NearMiss] aggregate (singular), backing
--                  INearMissRepository / NearMissDapperRepository (PlantSafety.Infrastructure.
--                  Features.SafetyEvents.NearMisses.Repositories). This is a DISTINCT table from
--                  [SafetyEvents].[NearMisses] (see NearMisses.sql), which backs the unrelated,
--                  separately-routed INearMissesRepository (api/v1/safety-events/near-misses --
--                  richer Report/StartReview/Complete/Close/Archive lifecycle, no Title/Description/
--                  HazardCategory columns). This singular feature is a simpler, independently-built
--                  aggregate (api/v1/features/safety-events/near-misses, CreateNearMissCommand,
--                  created 2026-06-17) that was never given SQL backing -- confirmed via a full
--                  scan of SafetyEvents/Sql/ finding no usp_NearMiss_* (singular) objects anywhere,
--                  while both API controllers/command handlers are real and independently routed.
--                  Column list mirrors INearMissRepository.NearMissReadModel and the
--                  StoredProcedureBuilder Add* calls in NearMissDapperRepository exactly. Status has
--                  no setter anywhere in INearMissRepository (Create/Update never pass it), so it is
--                  a fixed 'Reported' default written once at Create and never mutated by this
--                  feature. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NearMiss' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[NearMiss] (
        NearMissId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_NearMiss PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteId                  UNIQUEIDENTIFIER NOT NULL,
        Title                   NVARCHAR(200)    NOT NULL,
        Description             NVARCHAR(4000)   NOT NULL,
        OccurredAtUtc           DATETIME2(3)     NOT NULL,
        PotentialSeverity       NVARCHAR(50)     NOT NULL,
        HazardCategory          NVARCHAR(100)    NOT NULL,
        Status                  NVARCHAR(30)     NOT NULL CONSTRAINT DF_NearMiss_Status DEFAULT (N'Reported'),
        ReportedByWorkerId      UNIQUEIDENTIFIER NOT NULL,
        CreatedAtUtc            DATETIME2(3)     NOT NULL,
        CreatedBy               UNIQUEIDENTIFIER NOT NULL,
        LastModifiedByActorId   UNIQUEIDENTIFIER NULL,
        LastModifiedAtUtc       DATETIME2(3)     NOT NULL,
        RowVersion              ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_NearMiss_TenantId_OccurredAtUtc ON [SafetyEvents].[NearMiss] (TenantId, OccurredAtUtc);
    CREATE NONCLUSTERED INDEX IX_NearMiss_TenantId_SiteId ON [SafetyEvents].[NearMiss] (TenantId, SiteId);
END
GO
