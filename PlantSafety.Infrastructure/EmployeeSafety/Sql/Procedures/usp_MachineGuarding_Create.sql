-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.MachineGuarding
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MachineGuarding_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [EmployeeSafety].[MachineGuarding], backing
--                  IMachineGuardingRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_MachineGuarding_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @MachineGuardingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @GuardTitle NVARCHAR(400),
    @GuardType NVARCHAR(100),
    @EquipmentTag NVARCHAR(400),
    @Location NVARCHAR(400),
    @HasInterlock BIT,
    @InspectionIntervalDays INT,
    @LastInspectedByUserId UNIQUEIDENTIFIER = NULL,
    @LastInspectedAtUtc DATETIME2(3) = NULL,
    @NextInspectionDueAtUtc DATETIME2(3) = NULL,
    @OpenDeficiencyCount INT,
    @GuardConditionScore DECIMAL(9,2) = NULL,
    @BypassDetected BIT,
    @LastFindingNotes NVARCHAR(2000) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [EmployeeSafety].[MachineGuarding]
            ([MachineGuardingId], [TenantId], [SiteId], [Status], [GuardTitle], [GuardType], [EquipmentTag], [Location], [HasInterlock], [InspectionIntervalDays], [LastInspectedByUserId], [LastInspectedAtUtc], [NextInspectionDueAtUtc], [OpenDeficiencyCount], [GuardConditionScore], [BypassDetected], [LastFindingNotes], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@MachineGuardingId, @TenantId, @SiteId, @Status, @GuardTitle, @GuardType, @EquipmentTag, @Location, @HasInterlock, @InspectionIntervalDays, @LastInspectedByUserId, @LastInspectedAtUtc, @NextInspectionDueAtUtc, @OpenDeficiencyCount, @GuardConditionScore, @BypassDetected, @LastFindingNotes, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
