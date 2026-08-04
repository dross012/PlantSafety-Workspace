-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PublicSafetyAgency_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single PublicSafetyAgency row by identity, projecting RowVersion as
--                  a string ETag. Backs IPublicSafetyAgencyRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicSafetyAgency_GetById]
    @PublicSafetyAgencyId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT PublicSafetyAgencyId, TenantId, SiteId, Status, AgencyName, AgencyType, JurisdictionDescription,
        PrimaryPhoneNumber, PrimaryEmail, VerificationStatus, LastVerifiedAtUtc, NextVerificationDueAtUtc,
        CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [PublicSafetyCommunication].[PublicSafetyAgency]
    WHERE PublicSafetyAgencyId = @PublicSafetyAgencyId AND IsDeleted = 0;
END
GO
