-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.PreQualificationVetting
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PreQualificationVetting_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single PreQualificationVetting row by identifier, backing
--                  IPreQualificationVettingRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_PreQualificationVetting_GetById]
(
    @PreQualificationVettingId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [PreQualificationVettingId], [TenantId], [SiteId], [Status], [VettingTitle], [ContractorName], [EmrScore], [TrirScore], [OshaTrainingCompliant], [DeficiencyCount], [InsuranceExpiresAtUtc], [ReviewedByUserId], [VettedAtUtc], [ExpiresAtUtc], [RejectionReason], [CreatedAtUtc], [CreatedBy]
    FROM [ContractorSafety].[PreQualificationVetting]
    WHERE [PreQualificationVettingId] = @PreQualificationVettingId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
