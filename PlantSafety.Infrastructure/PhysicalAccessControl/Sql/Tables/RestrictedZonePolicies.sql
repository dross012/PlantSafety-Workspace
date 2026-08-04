-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.RestrictedZonePolicies
--  Layer:          Infrastructure - SQL Scripts
--  File:           RestrictedZonePolicies.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the RestrictedZonePolicy aggregate root backing
--                  IRestrictedZonePolicyRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RestrictedZonePolicies' AND schema_id = SCHEMA_ID('PhysicalAccessControl'))
BEGIN
    CREATE TABLE [PhysicalAccessControl].[RestrictedZonePolicies] (
        [RestrictedZonePolicyId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [SafetyZoneId] UNIQUEIDENTIFIER NOT NULL,
        [PolicyName] NVARCHAR(200) NOT NULL,
        [AllowedAccessLevel] NVARCHAR(100) NOT NULL,
        [EnforceAfterHoursLockout] BIT NOT NULL CONSTRAINT DF_RestrictedZonePolicies_EnforceAfterHoursLockout DEFAULT (0),
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_RestrictedZonePolicies_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RestrictedZonePolicies_TenantId_SafetyZoneId]
        ON [PhysicalAccessControl].[RestrictedZonePolicies] ([TenantId], [SafetyZoneId])
        INCLUDE ([Status], [AllowedAccessLevel]);
END
GO
