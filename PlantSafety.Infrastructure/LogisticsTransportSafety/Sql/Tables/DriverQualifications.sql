-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         LogisticsTransportSafety.DriverQualifications
--  Layer:          Infrastructure - SQL Scripts
--  File:           DriverQualifications.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code sql-database-skill agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the DriverQualification aggregate root backing
--                  IDriverQualificationRepository (Domain write repository) and the
--                  Application-layer read-model repository of the same simple name. [DriverUserId]
--                  references an Identity-module user and is intentionally NOT a foreign key here
--                  (cross-module reference; module-boundary rule -- see enforce-foreign-keys),
--                  but is indexed for the by-driver read-model query. Column list is
--                  column-for-column with DriverQualificationDapperRepository.DriverQualificationRow.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'LogisticsTransportSafety')
BEGIN
    EXEC('CREATE SCHEMA [LogisticsTransportSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DriverQualifications' AND schema_id = SCHEMA_ID('LogisticsTransportSafety'))
BEGIN
    CREATE TABLE [LogisticsTransportSafety].[DriverQualifications] (
        [DriverQualificationId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [DriverUserId] UNIQUEIDENTIFIER NOT NULL,
        [QualificationType] NVARCHAR(50) NOT NULL,
        [LicenseNumber] NVARCHAR(100) NOT NULL,
        [IssuedAtUtc] DATETIME2(3) NULL,
        [ExpiresAtUtc] DATETIME2(3) NULL,
        [SuspensionReason] NVARCHAR(1000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_DriverQualifications_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DriverQualifications_TenantId_DriverUserId]
        ON [LogisticsTransportSafety].[DriverQualifications] ([TenantId], [DriverUserId])
        INCLUDE ([SiteId], [Status], [QualificationType], [LicenseNumber], [ExpiresAtUtc]);

    CREATE NONCLUSTERED INDEX [IX_DriverQualifications_TenantId_SiteId]
        ON [LogisticsTransportSafety].[DriverQualifications] ([TenantId], [SiteId])
        INCLUDE ([Status], [QualificationType]);
END
GO
