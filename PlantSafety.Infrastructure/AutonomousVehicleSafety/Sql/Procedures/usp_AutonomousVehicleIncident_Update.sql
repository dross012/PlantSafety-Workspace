-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AutonomousVehicleIncident_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for
--                  [AutonomousVehicleSafety].[AutonomousVehicleIncident], backing
--                  IAutonomousVehicleIncidentRepository.UpdateAsync. Standard error envelope:
--                  TRY/CATCH with transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleIncident_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @AutonomousVehicleIncidentId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @AutonomousVehicleAssetId UNIQUEIDENTIFIER,
    @IncidentType NVARCHAR(100),
    @SeverityLevel NVARCHAR(100),
    @OccurredAtUtc DATETIME2(3),
    @DescriptionSummary NVARCHAR(2000),
    @InvestigationLeadUserId UNIQUEIDENTIFIER = NULL,
    @SuspensionTriggered BIT,
    @ClosedAtUtc DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [AutonomousVehicleSafety].[AutonomousVehicleIncident]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [AutonomousVehicleAssetId] = @AutonomousVehicleAssetId,
        [IncidentType] = @IncidentType,
        [SeverityLevel] = @SeverityLevel,
        [OccurredAtUtc] = @OccurredAtUtc,
        [DescriptionSummary] = @DescriptionSummary,
        [InvestigationLeadUserId] = @InvestigationLeadUserId,
        [SuspensionTriggered] = @SuspensionTriggered,
        [ClosedAtUtc] = @ClosedAtUtc,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [AutonomousVehicleIncidentId] = @AutonomousVehicleIncidentId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
