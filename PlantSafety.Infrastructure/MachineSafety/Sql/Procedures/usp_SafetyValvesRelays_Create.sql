-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.SafetyValvesRelays
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyValvesRelays_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [MachineSafety].[SafetyValvesRelays], backing
--                  ISafetyValvesRelaysRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_SafetyValvesRelays_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @SafetyValvesRelaysId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ComponentTitle NVARCHAR(400),
    @ComponentType NVARCHAR(100),
    @EquipmentLocation NVARCHAR(400),
    @InspectedByUserId UNIQUEIDENTIFIER,
    @InspectedAtUtc DATETIME2(3) = NULL,
    @NextInspectionDueAtUtc DATETIME2(3) = NULL,
    @FaultCount INT,
    @ReliabilityScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [MachineSafety].[SafetyValvesRelays]
            ([SafetyValvesRelaysId], [TenantId], [SiteId], [Status], [ComponentTitle], [ComponentType], [EquipmentLocation], [InspectedByUserId], [InspectedAtUtc], [NextInspectionDueAtUtc], [FaultCount], [ReliabilityScore], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@SafetyValvesRelaysId, @TenantId, @SiteId, @Status, @ComponentTitle, @ComponentType, @EquipmentLocation, @InspectedByUserId, @InspectedAtUtc, @NextInspectionDueAtUtc, @FaultCount, @ReliabilityScore, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
