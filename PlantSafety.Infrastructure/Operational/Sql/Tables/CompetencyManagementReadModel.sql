-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.CompetencyManagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           CompetencyManagementReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the CompetencyManagementReadModel read-model aggregate backing
--                  ICompetencyManagementRepository.GetCompetencyManagementAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CompetencyManagementReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[CompetencyManagementReadModel] (
        [CompetencyRecordId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EmployeeId]              UNIQUEIDENTIFIER    NOT NULL,
        [CompetencyArea]          NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [AssessedAtUtc]   DATETIME2(3)        NOT NULL,
        [AssessedBy]      UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CompetencyManagementReadModel_TenantId_AssessedAtUtc]
        ON [Operational].[CompetencyManagementReadModel] ([TenantId], [AssessedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[CompetencyManagementReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[CompetencyManagementReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO
