-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CommunitySafety.CommunityImpactAssessment
--  Layer:          Infrastructure - SQL Scripts
--  File:           CommunityImpactAssessment.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the CommunityImpactAssessment aggregate root backing
--                  ICommunityImpactAssessmentRepository. IncidentId is a weak reference (no FK)
--                  to [SafetyEvents].[Incident], per Zero Cross-Module Leakage. Additive-only
--                  evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'CommunitySafety')
BEGIN
    EXEC('CREATE SCHEMA [CommunitySafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CommunityImpactAssessment' AND schema_id = SCHEMA_ID('CommunitySafety'))
BEGIN
    CREATE TABLE [CommunitySafety].[CommunityImpactAssessment] (
        [CommunityImpactAssessmentId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NOT NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [IncidentId] UNIQUEIDENTIFIER NOT NULL,
        [Severity] NVARCHAR(30) NOT NULL,
        [AffectedArea] NVARCHAR(200) NOT NULL,
        [EstimatedPersonsAffected] INT NULL,
        [Description] NVARCHAR(4000) NOT NULL,
        [RequiresRightToKnowReporting] BIT NOT NULL CONSTRAINT DF_CommunityImpactAssessment_RTK DEFAULT (0),
        [AssessedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [AssessedAtUtc] DATETIME2(3) NOT NULL,
        [FinalizedAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_CommunityImpactAssessment_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CommunityImpactAssessment_TenantId_IncidentId]
        ON [CommunitySafety].[CommunityImpactAssessment] ([TenantId], [IncidentId])
        INCLUDE ([Status], [Severity]);
END
GO
