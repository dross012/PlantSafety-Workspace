-- ============================================================================
--  Module:         LogisticsTransportSafety.DriverQualifications
--  File:           usp_DriverQualification_GetStatusByDriver.sql
--  Created:        2026-08-04
--  Description:    Read-only procedure backing IDriverQualificationRepository (Application
--                  read-model).GetDriverQualificationStatusAsync, returning every qualification
--                  record held by a given driver within a tenant. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_DriverQualification_GetStatusByDriver]
    @TenantId UNIQUEIDENTIFIER,
    @DriverUserId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [DriverQualificationId], [TenantId], [SiteId], [DriverUserId], [QualificationType],
        [Status] AS [QualificationStatus], [LicenseNumber], [ExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[DriverQualifications]
    WHERE [TenantId] = @TenantId
      AND [DriverUserId] = @DriverUserId
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [ExpiresAtUtc] ASC;
END
GO
