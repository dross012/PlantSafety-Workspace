-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessAnomalies
--  Layer:          Infrastructure - SQL Scripts
--  File:           AccessAnomalies.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the AccessAnomaly aggregate root backing
--                  IAccessAnomalyRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AccessAnomalies' AND schema_id = SCHEMA_ID('PhysicalAccessControl'))
BEGIN
    CREATE TABLE [PhysicalAccessControl].[AccessAnomalies] (
        [AccessAnomalyId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [AccessEventId] UNIQUEIDENTIFIER NULL,
        [AccessPointId] UNIQUEIDENTIFIER NOT NULL,
        [CredentialId] UNIQUEIDENTIFIER NULL,
        [SafetyZoneId] UNIQUEIDENTIFIER NULL,
        [AnomalyType] NVARCHAR(100) NOT NULL,
        [SeverityScore] DECIMAL(9,2) NOT NULL,
        [AcknowledgedByUserId] UNIQUEIDENTIFIER NULL,
        [Resolution] NVARCHAR(2000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_AccessAnomalies_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AccessAnomalies_TenantId_SiteId_Status]
        ON [PhysicalAccessControl].[AccessAnomalies] ([TenantId], [SiteId], [Status])
        INCLUDE ([AnomalyType], [SeverityScore], [CreatedAtUtc]);
END
GO
