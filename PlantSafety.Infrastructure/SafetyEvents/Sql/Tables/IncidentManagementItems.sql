-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement
--  Layer:          Infrastructure – SQL Scripts
--  File:           IncidentManagementItems.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the IncidentManagement read-model, backing
--                  IIncidentManagementRepository.GetIncidentsAsync. Named
--                  IncidentManagementItems (not IncidentManagement) to avoid
--                  colliding with the unrelated [SafetyEvents].[usp_IncidentManagement_*]
--                  procedures already referenced by the separate Features-tree
--                  IncidentManagementRepository in this same module.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IncidentManagementItems' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[IncidentManagementItems] (
        IncidentId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        SiteId        UNIQUEIDENTIFIER NULL,
        IncidentType  NVARCHAR(50)     NOT NULL,
        Severity      NVARCHAR(50)     NOT NULL,
        Status        NVARCHAR(50)     NOT NULL,
        OccurredAtUtc DATETIME2(3)     NOT NULL,
        ReportedBy    UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_IncidentManagementItems_TenantId_SiteId] ON [SafetyEvents].[IncidentManagementItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_IncidentManagementItems_OccurredAtUtc] ON [SafetyEvents].[IncidentManagementItems] (OccurredAtUtc);
END
GO
