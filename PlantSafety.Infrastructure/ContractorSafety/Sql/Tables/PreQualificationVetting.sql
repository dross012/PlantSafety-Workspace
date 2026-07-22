-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.PreQualificationVetting
--  Layer:          Infrastructure - SQL Scripts
--  File:           PreQualificationVetting.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PreQualificationVetting aggregate root backing IPreQualificationVettingRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ContractorSafety')
BEGIN
    EXEC('CREATE SCHEMA [ContractorSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PreQualificationVetting' AND schema_id = SCHEMA_ID('ContractorSafety'))
BEGIN
    CREATE TABLE [ContractorSafety].[PreQualificationVetting] (
        [PreQualificationVettingId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [VettingTitle] NVARCHAR(400) NOT NULL,
        [ContractorName] NVARCHAR(400) NOT NULL,
        [EmrScore] DECIMAL(9,2) NULL,
        [TrirScore] DECIMAL(9,2) NULL,
        [OshaTrainingCompliant] BIT NOT NULL CONSTRAINT DF_PreQualificationVetting_OshaTrainingCompliant DEFAULT (0),
        [DeficiencyCount] INT NOT NULL CONSTRAINT DF_PreQualificationVetting_DeficiencyCount DEFAULT (0),
        [InsuranceExpiresAtUtc] DATETIME2(3) NULL,
        [ReviewedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [VettedAtUtc] DATETIME2(3) NULL,
        [ExpiresAtUtc] DATETIME2(3) NULL,
        [RejectionReason] NVARCHAR(2000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_PreQualificationVetting_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PreQualificationVetting_TenantId_SiteId]
        ON [ContractorSafety].[PreQualificationVetting] ([TenantId], [SiteId])
        INCLUDE ([Status], [VettingTitle]);
END
GO
