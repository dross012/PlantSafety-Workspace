-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Permits_GetAllActive.sql
--  Created:        2026-07-30
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted Permits rows. Backs IPermitsRepository.ListAsync,
--                  whose PermitsSpecification predicate is applied in-memory over the candidate
--                  set returned here (the Domain interface carries no tenant/site scoping
--                  parameter to push down to SQL). Not explicitly requested by the Phase 4
--                  build-out task list (which enumerated only Insert/Update/GetById/GetRange),
--                  but required so PermitsDapperRepository can implement the pre-existing
--                  IPermitsRepository.ListAsync member -- mirrors
--                  usp_PermitWorkflow_GetAllActive.sql / usp_Accident_GetAllActive.sql's identical
--                  precedent. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_Permits_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitsId, TenantId, SiteId, Status, PermitType, PermitNumber, WorkDescription, Location, HazardsIdentified,
        RequestedByUserId, ApprovedByUserId, IssuedAtUtc, ExpiresAtUtc, RejectedReason, RejectedAtUtc, CompletedAtUtc,
        ExpiredAtUtc, ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Compliance].[Permits]
    WHERE IsDeleted = 0;
END
GO
