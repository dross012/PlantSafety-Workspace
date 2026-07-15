-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ProceduresIncidentResponse
--  Layer:          Infrastructure - SQL Scripts
--  File:           ProceduresIncidentResponse.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ProceduresIncidentResponse read-model aggregate backing
--                  IIncidentResponseRepository.GetIncidentResponseProceduresAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresIncidentResponse' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresIncidentResponse] (
        [ProcedureId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [IncidentType]            NVARCHAR(300)       NOT NULL,
        [ResponseStep]            NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresIncidentResponse_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresIncidentResponse] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO