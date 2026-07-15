-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.AiGovernance
--  Layer:          Infrastructure - SQL Scripts
--  File:           GovernedModel.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the governed AI/ML model registry backing IAiGovernanceRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'GovernedModel' AND schema_id = SCHEMA_ID('AiGovernance'))
BEGIN
    CREATE TABLE [AiGovernance].[GovernedModel] (
        [ModelId]            UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]           UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]             UNIQUEIDENTIFIER    NULL,
        [ModelName]          NVARCHAR(200)       NOT NULL,
        [ModelVersion]       NVARCHAR(50)        NOT NULL,
        [GovernanceStatus]   NVARCHAR(50)        NOT NULL CONSTRAINT DF_GovernedModel_GovernanceStatus DEFAULT ('PendingReview'),
        [RiskTier]           NVARCHAR(20)        NOT NULL,
        [DriftStatus]        NVARCHAR(50)        NOT NULL CONSTRAINT DF_GovernedModel_DriftStatus DEFAULT ('Stable'),
        [LastReviewedAtUtc]  DATETIME2(3)        NOT NULL,
        [ReviewerName]       NVARCHAR(200)       NULL,
        [CreatedAtUtc]       DATETIME2(3)        NOT NULL,
        [CorrelationId]      UNIQUEIDENTIFIER    NOT NULL,
        [ActorId]            UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_GovernedModel_TenantId_SiteId]
        ON [AiGovernance].[GovernedModel] ([TenantId], [SiteId])
        INCLUDE ([ModelName], [ModelVersion], [GovernanceStatus], [RiskTier], [DriftStatus]);
END
GO
