-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.SiteOrientation
--  Layer:          Infrastructure - SQL Scripts
--  File:           SiteOrientation.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the SiteOrientation aggregate root backing ISiteOrientationRepository.
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SiteOrientation' AND schema_id = SCHEMA_ID('ContractorSafety'))
BEGIN
    CREATE TABLE [ContractorSafety].[SiteOrientation] (
        [SiteOrientationId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [OrientationTitle] NVARCHAR(400) NOT NULL,
        [ContractorName] NVARCHAR(400) NOT NULL,
        [ConductedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [DeliveredAtUtc] DATETIME2(3) NULL,
        [AttendeeCount] INT NOT NULL CONSTRAINT DF_SiteOrientation_AttendeeCount DEFAULT (0),
        [ComprehensionScore] DECIMAL(9,2) NULL,
        [PassingScoreThreshold] DECIMAL(9,2) NOT NULL,
        [ExpiresAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_SiteOrientation_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_SiteOrientation_TenantId_SiteId]
        ON [ContractorSafety].[SiteOrientation] ([TenantId], [SiteId])
        INCLUDE ([Status], [OrientationTitle]);
END
GO
