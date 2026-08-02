-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.Observation
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ObservationItems_GetAllActive.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted ObservationItems rows. Backs
--                  IObservationRepository.ListAsync (Domain write-side repository), whose
--                  ObservationSpecification predicate is applied in-memory over the candidate
--                  set returned here (the Domain interface carries no tenant/site scoping
--                  parameter to push down to SQL). Mirrors usp_Permits_GetAllActive.sql.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_ObservationItems_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ObservationId, TenantId, SiteId, Status, ObservationType, Category, Location, Description,
        CorrectiveActionRequired, FollowUpDueAtUtc, ObservedAtUtc, ObservedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [SafetyEvents].[ObservationItems]
    WHERE IsDeleted = 0;
END
GO
