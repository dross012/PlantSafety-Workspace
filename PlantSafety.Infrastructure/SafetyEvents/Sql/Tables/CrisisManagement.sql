-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.CrisisManagement
--  Layer:          Infrastructure – SQL Scripts
--  File:           CrisisManagement.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the CrisisManagement read-model, backing
--                  ICrisisManagementRepository.GetCrisisEventsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CrisisManagement' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[CrisisManagement] (
        CrisisId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        SiteId        UNIQUEIDENTIFIER NULL,
        Title         NVARCHAR(200)    NOT NULL,
        Severity      NVARCHAR(50)     NOT NULL,
        Status        NVARCHAR(50)     NOT NULL,
        DeclaredAtUtc DATETIME2(3)     NOT NULL,
        DeclaredBy    UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CrisisManagement_TenantId_SiteId] ON [SafetyEvents].[CrisisManagement] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CrisisManagement_DeclaredAtUtc] ON [SafetyEvents].[CrisisManagement] (DeclaredAtUtc);
END
GO
