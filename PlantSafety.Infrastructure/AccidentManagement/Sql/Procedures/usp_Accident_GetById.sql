-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AccidentManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Accident_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single Accident aggregate root row by identity. Backs
--                  IAccidentRepository.GetByIdAsync. Additive-only evolution.
--  UPDATED:        2026-07-28 -- added ETag, projected from the new RowVersion column the same way
--                  as usp_IncidentRead_GetById.sql / usp_Culture_GetById.sql: CONVERT(NVARCHAR(34),
--                  CAST(RowVersion AS BINARY(8)), 1).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_Accident_GetById]
    @AccidentId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        AccidentId, TenantId, SiteId, Status, AccidentTitle, Location, Severity, InjuredPersonCount,
        OccurredAtUtc, Category, SiteSecuredAndTriaged, InvestigatorId, InvestigatorAssignedAtUtc,
        RootCause, RootCauseNotes, RootCauseRecordedAtUtc, RegulatoryFilingRequired, RegulatoryAgency,
        RegulatoryReferenceNumber, RegulatoryFiledAtUtc, ClosedAtUtc, OwnerId, ReviewRequired,
        CreatedAtUtc, CreatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [AccidentManagement].[Accident]
    -- App-layer tenant scoping (defense-in-depth alongside RLS, per F1 in
    -- docs\lifecycle\security\rls-audit.md): explicit WHERE clause, not reliance on RLS alone.
    WHERE AccidentId = @AccidentId AND TenantId = @TenantId AND IsDeleted = 0;
END
GO
