-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.SafetyCaseValidation
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyCaseValidation_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single SafetyCaseValidation row by identifier, backing
--                  ISafetyCaseValidationRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_SafetyCaseValidation_GetById]
(
    @SafetyCaseValidationId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [SafetyCaseValidationId], [TenantId], [SiteId], [Status], [AutonomousVehicleAssetId], [StandardReference], [DocumentVersion], [RevalidationCadenceDays], [ValidatedByUserId], [ValidatedAtUtc], [ExpiresAtUtc], [RejectionReason], [CreatedAtUtc], [CreatedBy]
    FROM [AutonomousVehicleSafety].[SafetyCaseValidation]
    WHERE [SafetyCaseValidationId] = @SafetyCaseValidationId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
