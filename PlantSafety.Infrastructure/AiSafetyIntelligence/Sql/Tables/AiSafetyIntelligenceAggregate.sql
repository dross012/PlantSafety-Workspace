-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AiSafetyIntelligence
--  Layer:          Infrastructure -- SQL Scripts
--  File:           AiSafetyIntelligenceAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the AiSafetyIntelligence aggregate root
--                  (IAiSafetyIntelligenceRepository). Tenant-scoped (TenantId NOT NULL
--                  uniqueidentifier). Phase 4 full build-out: this aggregate previously had zero
--                  persistence at all. Mirrors [Compliance].[AuditingAggregate]'s native ROWVERSION
--                  concurrency convention from day one. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AiSafetyIntelligence')
    EXEC('CREATE SCHEMA AiSafetyIntelligence AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AiSafetyIntelligenceAggregate' AND schema_id = SCHEMA_ID('AiSafetyIntelligence'))
BEGIN
    CREATE TABLE [AiSafetyIntelligence].[AiSafetyIntelligenceAggregate]
    (
        AiSafetyIntelligenceId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AiSafetyIntelligenceAggregate PRIMARY KEY,
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        SiteId                   UNIQUEIDENTIFIER NULL,
        Status                   NVARCHAR(50)     NOT NULL,
        InsightType              NVARCHAR(50)     NOT NULL,
        Description              NVARCHAR(2000)   NOT NULL,
        ConfidenceScore          DECIMAL(5,4)     NOT NULL,
        SourceModelName          NVARCHAR(200)    NOT NULL,
        CreatedAtUtc             DATETIME2(3)     NOT NULL,
        CreatedBy                UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc             DATETIME2(3)     NULL,
        UpdatedBy                UNIQUEIDENTIFIER NULL,
        IsDeleted                BIT              NOT NULL CONSTRAINT DF_AiSafetyIntelligenceAggregate_IsDeleted DEFAULT (0),
        RowVersion               ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_AiSafetyIntelligenceAggregate_TenantId_Site ON [AiSafetyIntelligence].[AiSafetyIntelligenceAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_AiSafetyIntelligenceAggregate_Status ON [AiSafetyIntelligence].[AiSafetyIntelligenceAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_AiSafetyIntelligenceAggregate_CreatedAtUtc ON [AiSafetyIntelligence].[AiSafetyIntelligenceAggregate] (CreatedAtUtc);
END
GO
