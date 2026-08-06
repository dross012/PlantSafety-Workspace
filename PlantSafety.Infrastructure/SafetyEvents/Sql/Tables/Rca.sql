-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.RootCauseAnalysis
--  Layer:          Infrastructure – SQL Scripts
--  File:           Rca.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the [SafetyEvents].[Rca] aggregate, backing IRcaRepository /
--                  RcaDapperRepository (PlantSafety.Infrastructure.Features.SafetyEvents.
--                  RootCauseAnalysis.Repositories). Same dual/triple-implementation situation
--                  discovered for NearMiss/NearMisses and Observation (see NearMiss.sql,
--                  Observation.sql): this Features-namespaced aggregate (CreateRcaCommand, created
--                  2026-06-17, real route api/v1/features/safety-events/root-cause-analysis) never
--                  got SQL backing. DISTINCT from [SafetyEvents].[RootCauseAnalysis] (AnalysisId/
--                  RelatedEventId/LeadAnalyst/Submitted-Approved-Closed lifecycle -- a separate,
--                  richer feature backing the other api/v1/safety-events/root-cause-analysis V1/V2
--                  controllers) and from [ContinuousImprovement].[RCA]. Column list mirrors
--                  RcaReadModel and the StoredProcedureBuilder Add* calls in RcaDapperRepository
--                  exactly. IRcaRepository exposes only Create/GetById (no Update/List/Count), so
--                  Status is a fixed 'Recorded' default written once at Create. Additive-only
--                  evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Rca' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[Rca] (
        RcaId                UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Rca PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        LinkedEventId         UNIQUEIDENTIFIER NOT NULL,
        LinkedEventType       NVARCHAR(100)    NOT NULL,
        Methodology           NVARCHAR(100)    NOT NULL,
        RootCauses            NVARCHAR(MAX)    NOT NULL,
        CorrectiveActions     NVARCHAR(MAX)    NOT NULL,
        LeadInvestigatorId    UNIQUEIDENTIFIER NOT NULL,
        Status                NVARCHAR(30)     NOT NULL CONSTRAINT DF_Rca_Status DEFAULT (N'Recorded'),
        CreatedAtUtc          DATETIME2(3)     NOT NULL,
        CreatedBy             UNIQUEIDENTIFIER NOT NULL,
        LastModifiedByActorId UNIQUEIDENTIFIER NULL,
        LastModifiedAtUtc     DATETIME2(3)     NOT NULL,
        RowVersion            ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Rca_TenantId_LinkedEventId ON [SafetyEvents].[Rca] (TenantId, LinkedEventId);
END
GO
