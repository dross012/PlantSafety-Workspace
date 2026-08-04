-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PublicSafetyAgency_ListByTenant.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IPublicSafetyAgencyRepository (Application
--                  layer read model). Supports the ListPublicSafetyAgencies query and the Agency
--                  Notification Status Board dashboard. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicSafetyAgency_ListByTenant]
    @TenantId    UNIQUEIDENTIFIER,
    @SiteId      UNIQUEIDENTIFIER = NULL,
    @AgencyType  NVARCHAR(64) = NULL,
    @ActiveOnly  BIT = 0
AS
SET NOCOUNT ON;
BEGIN
    SELECT PublicSafetyAgencyId, TenantId, SiteId, Status, AgencyName, AgencyType, JurisdictionDescription,
        PrimaryPhoneNumber, PrimaryEmail, VerificationStatus, LastVerifiedAtUtc, NextVerificationDueAtUtc, CreatedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [PublicSafetyCommunication].[PublicSafetyAgency]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND (@AgencyType IS NULL OR AgencyType = @AgencyType)
      AND (@ActiveOnly = 0 OR Status = N'Active')
      AND IsDeleted = 0
    ORDER BY AgencyName ASC;
END
GO
