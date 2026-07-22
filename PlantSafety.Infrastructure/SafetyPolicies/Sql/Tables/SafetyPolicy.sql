-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPolicies
--  Layer:          Infrastructure -- SQL Scripts
--  File:           SafetyPolicy.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the SafetyPolicy aggregate root (ISafetyPolicyRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier) so the platform-wide
--                  Row-Level-Security discovery in 90-row-level-security.sql picks it up
--                  automatically. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'SafetyPolicies')
    EXEC('CREATE SCHEMA SafetyPolicies AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SafetyPolicy' AND schema_id = SCHEMA_ID('SafetyPolicies'))
BEGIN
    CREATE TABLE [SafetyPolicies].[SafetyPolicy]
    (
        SafetyPolicyId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SafetyPolicy PRIMARY KEY,
        TenantId                    UNIQUEIDENTIFIER NOT NULL,
        SiteId                      UNIQUEIDENTIFIER NULL,
        Status                      NVARCHAR(30)     NOT NULL,
        SafetyPolicyTitle           NVARCHAR(400)    NOT NULL,
        PolicyNumber                NVARCHAR(100)    NOT NULL,
        SafetyPolicyVersion         INT              NOT NULL CONSTRAINT DF_SafetyPolicy_Version DEFAULT (1),
        Category                    NVARCHAR(100)    NOT NULL,
        Applicability               NVARCHAR(400)    NOT NULL,
        OwnerId                     UNIQUEIDENTIFIER NOT NULL,
        ApprovedBy                  UNIQUEIDENTIFIER NULL,
        ApprovedAtUtc               DATETIME2(3)     NULL,
        EffectiveAtUtc              DATETIME2(3)     NULL,
        ReviewCadence               NVARCHAR(30)     NOT NULL,
        LastReviewedAtUtc           DATETIME2(3)     NULL,
        NextReviewDueUtc            DATETIME2(3)     NULL,
        ReviewRequired              BIT              NOT NULL CONSTRAINT DF_SafetyPolicy_ReviewRequired DEFAULT (1),
        RequiredAcknowledgmentCount INT              NOT NULL CONSTRAINT DF_SafetyPolicy_ReqAckCount DEFAULT (0),
        ReviewCount                 INT              NOT NULL CONSTRAINT DF_SafetyPolicy_ReviewCount DEFAULT (0),
        CreatedAtUtc                DATETIME2(3)     NOT NULL,
        CreatedBy                   UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc                DATETIME2(3)     NULL,
        UpdatedBy                   UNIQUEIDENTIFIER NULL,
        IsDeleted                   BIT              NOT NULL CONSTRAINT DF_SafetyPolicy_IsDeleted DEFAULT (0),
        ConcurrencyToken            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SafetyPolicy_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_SafetyPolicy_TenantId_Category ON [SafetyPolicies].[SafetyPolicy] (TenantId, Category, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_SafetyPolicy_TenantId_Site ON [SafetyPolicies].[SafetyPolicy] (TenantId, SiteId, IsDeleted);
END
GO
