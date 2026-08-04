-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.Credentials
--  Layer:          Infrastructure - SQL Scripts
--  File:           Credentials.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Credential aggregate root backing ICredentialRepository.
--                  Badge/credential PII fields are access-restricted and audit-logged; see
--                  docs/architecture/audit-logging.md and the module's data-governance policy.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Credentials' AND schema_id = SCHEMA_ID('PhysicalAccessControl'))
BEGIN
    CREATE TABLE [PhysicalAccessControl].[Credentials] (
        [CredentialId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [BadgeNumber] NVARCHAR(100) NOT NULL,
        [HolderUserId] UNIQUEIDENTIFIER NOT NULL,
        [HolderType] NVARCHAR(100) NOT NULL,
        [AccessLevel] NVARCHAR(100) NOT NULL,
        [IssuedAtUtc] DATETIME2(3) NOT NULL,
        [ExpiresAtUtc] DATETIME2(3) NULL,
        [RevocationReason] NVARCHAR(1000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_Credentials_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Credentials_TenantId_HolderUserId]
        ON [PhysicalAccessControl].[Credentials] ([TenantId], [HolderUserId])
        INCLUDE ([Status], [AccessLevel]);
END
GO
