-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.SafetyCaseValidation
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyCaseValidation_GetStatusByAssetId.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ISafetyCaseValidationRepository.GetSafetyCaseStatusByAssetIdAsync (Application
--                  read-model, GetSafetyCaseStatus query). Returns the single most recently
--                  created safety-case validation for the asset (prior document versions remain
--                  in the table for audit but are not surfaced by this "current status" query).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_SafetyCaseValidation_GetStatusByAssetId]
    @TenantId UNIQUEIDENTIFIER,
    @AutonomousVehicleAssetId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT TOP (1)
        [SafetyCaseValidationId],
        [TenantId],
        [SiteId],
        [AutonomousVehicleAssetId],
        [StandardReference],
        [DocumentVersion],
        [Status] AS [Status],
        [ValidatedAtUtc],
        [ExpiresAtUtc],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [AutonomousVehicleSafety].[SafetyCaseValidation]
    WHERE [TenantId] = @TenantId
      AND [AutonomousVehicleAssetId] = @AutonomousVehicleAssetId
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [CreatedAtUtc] DESC;
END
GO
