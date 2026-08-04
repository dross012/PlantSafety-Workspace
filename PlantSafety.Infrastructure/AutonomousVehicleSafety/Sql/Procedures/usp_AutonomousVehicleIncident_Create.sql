-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AutonomousVehicleIncident_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [AutonomousVehicleSafety].[AutonomousVehicleIncident],
--                  backing IAutonomousVehicleIncidentRepository.AddAsync. Standard error
--                  envelope: TRY/CATCH with transaction rollback and THROW. Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleIncident_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
    @ClosedAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [AutonomousVehicleSafety].[AutonomousVehicleIncident]
            ([AutonomousVehicleIncidentId], [TenantId], [SiteId], [Status], [AutonomousVehicleAssetId], [IncidentType], [SeverityLevel], [OccurredAtUtc], [DescriptionSummary], [InvestigationLeadUserId], [SuspensionTriggered], [ClosedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@AutonomousVehicleIncidentId, @TenantId, @SiteId, @Status, @AutonomousVehicleAssetId, @IncidentType, @SeverityLevel, @OccurredAtUtc, @DescriptionSummary, @InvestigationLeadUserId, @SuspensionTriggered, @ClosedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
