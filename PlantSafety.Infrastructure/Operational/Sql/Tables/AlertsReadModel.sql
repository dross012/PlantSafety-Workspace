-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Alerts
--  Layer:          Infrastructure - SQL Scripts
--  File:           AlertsReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the AlertsReadModel read-model aggregate backing
--                  IAlertsRepository.GetAlertsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AlertsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[AlertsReadModel] (
        [AlertId]         UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [Message]                 NVARCHAR(400)       NOT NULL,
        [Severity]                NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [TriggeredAtUtc]  DATETIME2(3)        NOT NULL,
        [AcknowledgedBy]  UNIQUEIDENTIFIER    NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AlertsReadModel_TenantId_TriggeredAtUtc]
        ON [Operational].[AlertsReadModel] ([TenantId], [TriggeredAtUtc])
        INCLUDE ([SiteId]);
END
GO
