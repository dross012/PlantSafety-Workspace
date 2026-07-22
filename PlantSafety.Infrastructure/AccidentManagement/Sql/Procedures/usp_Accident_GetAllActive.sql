-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AccidentManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Accident_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted Accident aggregate root rows. Backs
--                  IAccidentRepository.ListAsync, whose AccidentSpecification predicate is
--                  applied in-memory over the candidate set returned here.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_Accident_GetAllActive]
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
    WHERE IsDeleted = 0;
END
GO
