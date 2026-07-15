-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.Commands
--  Layer:          Infrastructure – SQL Scripts
--  File:           SafetyEventsResources.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Generic cross-aggregate resource table backing
--                  ISafetyEventsBulkRepository.BulkUpdateAsync. Bulk operations
--                  ("Close", "Approve", "Reassign", etc.) address SafetyEvents
--                  resources by Id irrespective of which specific SafetyEvents
--                  aggregate (Incident, NearMiss, Observation, etc.) they
--                  originated from.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SafetyEventsResources' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[SafetyEventsResources] (
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

    CREATE NONCLUSTERED INDEX [IX_SafetyEventsResources_TenantId_SiteId] ON [SafetyEvents].[SafetyEventsResources] (TenantId, SiteId);
END
GO
