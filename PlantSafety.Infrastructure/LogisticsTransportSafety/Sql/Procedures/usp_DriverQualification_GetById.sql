-- ============================================================================
--  Module:         LogisticsTransportSafety.DriverQualifications
--  File:           usp_DriverQualification_GetById.sql
--  Created:        2026-08-04
--  Description:    Returns a single DriverQualification row by identifier, backing
--                  IDriverQualificationRepository.GetByIdAsync (Domain, id is globally unique so
--                  no tenant filter is required for correctness). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_DriverQualification_GetById]
    @DriverQualificationId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [DriverQualificationId], [TenantId], [SiteId], [Status], [DriverUserId], [QualificationType],
        [LicenseNumber], [IssuedAtUtc], [ExpiresAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[DriverQualifications]
    WHERE [DriverQualificationId] = @DriverQualificationId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
