-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.DrillsAndExercises
--  Layer:          Infrastructure – SQL Scripts
--  File:           DrillsAndExercises.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the DrillsAndExercises read-model, backing
--                  IDrillsAndExercisesRepository.GetDrillsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DrillsAndExercises' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[DrillsAndExercises] (
        DrillId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        DrillType        NVARCHAR(50)     NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        ConductedAtUtc   DATETIME2(3)     NOT NULL,
        ConductedBy      UNIQUEIDENTIFIER NOT NULL,
        ParticipantCount INT              NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DrillsAndExercises_TenantId_SiteId] ON [SafetyEvents].[DrillsAndExercises] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_DrillsAndExercises_ConductedAtUtc] ON [SafetyEvents].[DrillsAndExercises] (ConductedAtUtc);
END
GO
