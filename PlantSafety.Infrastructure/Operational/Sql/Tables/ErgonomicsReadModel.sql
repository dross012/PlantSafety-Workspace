-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Ergonomics
--  Layer:          Infrastructure - SQL Scripts
--  File:           ErgonomicsReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ErgonomicsReadModel read-model aggregate backing
--                  IErgonomicsRepository.GetErgonomicsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ErgonomicsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[ErgonomicsReadModel] (
        [ErgonomicsAssessmentId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [WorkstationId]           UNIQUEIDENTIFIER    NOT NULL,
        [RiskLevel]               NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [AssessedAtUtc]   DATETIME2(3)        NOT NULL,
        [AssessedBy]      UNIQUEIDENTIFIER    NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ErgonomicsReadModel_TenantId_AssessedAtUtc]
        ON [Operational].[ErgonomicsReadModel] ([TenantId], [AssessedAtUtc])
        INCLUDE ([SiteId]);
END
GO
