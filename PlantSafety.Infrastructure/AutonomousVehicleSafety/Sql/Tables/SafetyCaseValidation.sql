-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.SafetyCaseValidation
--  Layer:          Infrastructure - SQL Scripts
--  File:           SafetyCaseValidation.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the SafetyCaseValidation aggregate root backing
--                  ISafetyCaseValidationRepository. Each row is a distinct, version-controlled
--                  safety-case document validation; prior rows are never overwritten, satisfying
--                  the module's "prior versions retained for audit" data-governance policy.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate). Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AutonomousVehicleSafety')
BEGIN
    EXEC('CREATE SCHEMA [AutonomousVehicleSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SafetyCaseValidation' AND schema_id = SCHEMA_ID('AutonomousVehicleSafety'))
BEGIN
    CREATE TABLE [AutonomousVehicleSafety].[SafetyCaseValidation] (
        [SafetyCaseValidationId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [AutonomousVehicleAssetId] UNIQUEIDENTIFIER NOT NULL,
        [StandardReference] NVARCHAR(100) NOT NULL,
        [DocumentVersion] NVARCHAR(50) NOT NULL,
        [RevalidationCadenceDays] INT NOT NULL,
        [ValidatedByUserId] UNIQUEIDENTIFIER NULL,
        [ValidatedAtUtc] DATETIME2(3) NULL,
        [ExpiresAtUtc] DATETIME2(3) NULL,
        [RejectionReason] NVARCHAR(1000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_SafetyCaseValidation_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_SafetyCaseValidation_TenantId_AssetId]
        ON [AutonomousVehicleSafety].[SafetyCaseValidation] ([TenantId], [AutonomousVehicleAssetId])
        INCLUDE ([Status], [ExpiresAtUtc], [CreatedAtUtc]);
END
GO
