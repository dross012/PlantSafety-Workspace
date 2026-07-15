-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.EmergencyManagement
--  Layer:          Infrastructure – SQL Scripts
--  File:           EmergencyManagementItems.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EmergencyManagement read-model, backing
--                  IEmergencyManagementRepository.GetEmergenciesAsync. Named
--                  EmergencyManagementItems (not EmergencyManagement) to avoid
--                  colliding with the unrelated [SafetyEvents].[usp_EmergencyManagement_*]
--                  procedures already referenced by the separate Features-tree
--                  EmergencyManagementRepository in this same module.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmergencyManagementItems' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[EmergencyManagementItems] (
        EmergencyId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId       UNIQUEIDENTIFIER NOT NULL,
        SiteId         UNIQUEIDENTIFIER NULL,
        EmergencyType  NVARCHAR(50)     NOT NULL,
        Severity       NVARCHAR(50)     NOT NULL,
        Status         NVARCHAR(50)     NOT NULL,
        ReportedAtUtc  DATETIME2(3)     NOT NULL,
        ReportedBy     UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EmergencyManagementItems_TenantId_SiteId] ON [SafetyEvents].[EmergencyManagementItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_EmergencyManagementItems_ReportedAtUtc] ON [SafetyEvents].[EmergencyManagementItems] (ReportedAtUtc);
END
GO
