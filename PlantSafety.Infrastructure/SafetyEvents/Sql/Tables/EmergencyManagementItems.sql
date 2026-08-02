-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.EmergencyManagement
--  Layer:          Infrastructure – SQL Scripts
--  File:           EmergencyManagementItems.sql
--  Created:        2026-07-13
--  Updated:        2026-08-02 -- Phase 4 full build-out: additively extended with the columns
--                  needed to persist the full EmergencyManagement aggregate write path
--                  (Location/CasualtyCount/ResponseTeamNotified/StabilizedAtUtc/ResolvedAtUtc/
--                  UpdatedAtUtc/UpdatedBy/IsDeleted/RowVersion), backing the new
--                  usp_EmergencyManagementItems_Insert/_Update/_GetById procedures and
--                  IEmergencyManagementRepository (Domain write-side aggregate repository).
--                  Pre-existing columns are reused for overlapping aggregate properties rather
--                  than duplicated: EmergencyId serves as EmergencyManagementId, ReportedAtUtc
--                  serves as both DeclaredAtUtc and CreatedAtUtc (they are always equal at
--                  creation and DeclaredAtUtc never changes afterward), and ReportedBy serves as
--                  CreatedBy. All ALTER guards are idempotent (IF NOT EXISTS on sys.columns).
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EmergencyManagement read-model, backing
--                  IEmergencyManagementRepository.GetEmergenciesAsync (read) and, following this
--                  update, the full aggregate write path (Insert/Update/GetById). Named
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

-- ---- Phase 4 additive columns (aggregate write path) ------------------------

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[EmergencyManagementItems]') AND name = 'Location')
    ALTER TABLE [SafetyEvents].[EmergencyManagementItems] ADD Location NVARCHAR(400) NOT NULL CONSTRAINT DF_EmergencyManagementItems_Location DEFAULT ('');
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[EmergencyManagementItems]') AND name = 'CasualtyCount')
    ALTER TABLE [SafetyEvents].[EmergencyManagementItems] ADD CasualtyCount INT NOT NULL CONSTRAINT DF_EmergencyManagementItems_CasualtyCount DEFAULT (0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[EmergencyManagementItems]') AND name = 'ResponseTeamNotified')
    ALTER TABLE [SafetyEvents].[EmergencyManagementItems] ADD ResponseTeamNotified BIT NOT NULL CONSTRAINT DF_EmergencyManagementItems_ResponseTeamNotified DEFAULT (0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[EmergencyManagementItems]') AND name = 'StabilizedAtUtc')
    ALTER TABLE [SafetyEvents].[EmergencyManagementItems] ADD StabilizedAtUtc DATETIME2(3) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[EmergencyManagementItems]') AND name = 'ResolvedAtUtc')
    ALTER TABLE [SafetyEvents].[EmergencyManagementItems] ADD ResolvedAtUtc DATETIME2(3) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[EmergencyManagementItems]') AND name = 'UpdatedAtUtc')
    ALTER TABLE [SafetyEvents].[EmergencyManagementItems] ADD UpdatedAtUtc DATETIME2(3) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[EmergencyManagementItems]') AND name = 'UpdatedBy')
    ALTER TABLE [SafetyEvents].[EmergencyManagementItems] ADD UpdatedBy UNIQUEIDENTIFIER NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[EmergencyManagementItems]') AND name = 'IsDeleted')
    ALTER TABLE [SafetyEvents].[EmergencyManagementItems] ADD IsDeleted BIT NOT NULL CONSTRAINT DF_EmergencyManagementItems_IsDeleted DEFAULT (0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[EmergencyManagementItems]') AND name = 'RowVersion')
    ALTER TABLE [SafetyEvents].[EmergencyManagementItems] ADD RowVersion ROWVERSION NOT NULL;
GO
