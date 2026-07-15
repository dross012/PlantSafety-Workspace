-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Commands
--  Layer:          Infrastructure – SQL Scripts
--  File:           ComplianceResources.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Generic cross-aggregate resource table backing
--                  IComplianceBulkRepository.BulkUpdateAsync. Bulk operations
--                  ("Close", "Approve", "Reassign", etc.) address Compliance
--                  resources by Id irrespective of which specific Compliance
--                  aggregate they originated from.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComplianceResources' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ComplianceResources] (
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

    CREATE NONCLUSTERED INDEX [IX_ComplianceResources_TenantId_SiteId] ON [Compliance].[ComplianceResources] (TenantId, SiteId);
END
GO
