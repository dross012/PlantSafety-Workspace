-- ============================================================================
--  Module:         LogisticsTransportSafety.DriverQualifications
--  File:           usp_DriverQualification_GetAll.sql
--  Created:        2026-08-04
--  Description:    Returns all non-deleted DriverQualification rows, backing
--                  IDriverQualificationRepository.ListAsync (the Domain specification predicate is
--                  applied in-memory in the Infrastructure repository against this full result
--                  set). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_DriverQualification_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [DriverQualificationId], [TenantId], [SiteId], [Status], [DriverUserId], [QualificationType],
        [LicenseNumber], [IssuedAtUtc], [ExpiresAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[DriverQualifications]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
