-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Commands
--  Layer:          Infrastructure - SQL Scripts
--  File:           OperationalResources.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Generic cross-aggregate resource table backing
--                  IOperationalBulkRepository.BulkUpdateAsync. Bulk operations
--                  ("Close", "Approve", "Reassign", etc.) address Operational
--                  resources by Id irrespective of which specific Operational
--                  aggregate they originated from.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OperationalResources' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[OperationalResources] (
        Id               UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        ResourceType     NVARCHAR(100)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        AssignedToUserId UNIQUEIDENTIFIER NULL,
        Notes            NVARCHAR(MAX)    NULL,
        UpdatedAtUtc     DATETIME2(3)     NOT NULL,
        UpdatedBy        UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_OperationalResources_TenantId_SiteId] ON [Operational].[OperationalResources] (TenantId, SiteId);
END
GO
