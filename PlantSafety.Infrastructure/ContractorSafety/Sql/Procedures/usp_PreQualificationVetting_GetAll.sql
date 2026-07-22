-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.PreQualificationVetting
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PreQualificationVetting_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PreQualificationVetting rows, backing
--                  IPreQualificationVettingRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_PreQualificationVetting_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [PreQualificationVettingId], [TenantId], [SiteId], [Status], [VettingTitle], [ContractorName], [EmrScore], [TrirScore], [OshaTrainingCompliant], [DeficiencyCount], [InsuranceExpiresAtUtc], [ReviewedByUserId], [VettedAtUtc], [ExpiresAtUtc], [RejectionReason], [CreatedAtUtc], [CreatedBy]
    FROM [ContractorSafety].[PreQualificationVetting]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
