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
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_Accident_GetById]
    @AccidentId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        AccidentId, TenantId, SiteId, Status, AccidentTitle, Location, Severity, InjuredPersonCount,
        OccurredAtUtc, Category, SiteSecuredAndTriaged, InvestigatorId, InvestigatorAssignedAtUtc,
        RootCause, RootCauseNotes, RootCauseRecordedAtUtc, RegulatoryFilingRequired, RegulatoryAgency,
        RegulatoryReferenceNumber, RegulatoryFiledAtUtc, ClosedAtUtc, OwnerId, ReviewRequired,
        CreatedAtUtc, CreatedBy
    FROM [AccidentManagement].[Accident]
    WHERE AccidentId = @AccidentId AND IsDeleted = 0;
END
GO
