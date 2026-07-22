-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.TwoHandControls
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_TwoHandControls_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [MachineSafety].[TwoHandControls], backing
--                  ITwoHandControlsRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_TwoHandControls_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @TwoHandControlsId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ControlTitle NVARCHAR(400),
    @EquipmentType NVARCHAR(100),
    @EquipmentLocation NVARCHAR(400),
    @TestedByUserId UNIQUEIDENTIFIER,
    @TestedAtUtc DATETIME2(3) = NULL,
    @NextTestDueAtUtc DATETIME2(3) = NULL,
    @BypassAttemptCount INT,
    @SynchronizationScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [MachineSafety].[TwoHandControls]
            ([TwoHandControlsId], [TenantId], [SiteId], [Status], [ControlTitle], [EquipmentType], [EquipmentLocation], [TestedByUserId], [TestedAtUtc], [NextTestDueAtUtc], [BypassAttemptCount], [SynchronizationScore], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@TwoHandControlsId, @TenantId, @SiteId, @Status, @ControlTitle, @EquipmentType, @EquipmentLocation, @TestedByUserId, @TestedAtUtc, @NextTestDueAtUtc, @BypassAttemptCount, @SynchronizationScore, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
