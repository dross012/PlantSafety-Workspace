-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.PermitToWork
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PermitToWork_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [ContractorSafety].[PermitToWork], backing
--                  IPermitToWorkRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_PermitToWork_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @PermitToWorkId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @PermitTitle NVARCHAR(400),
    @PermitType NVARCHAR(100),
    @WorkLocation NVARCHAR(400),
    @HazardCount INT,
    @RequiresAtmosphericTesting BIT,
    @AtmosphericTestScore DECIMAL(9,2) = NULL,
    @AuthorizedByUserId UNIQUEIDENTIFIER,
    @IssuedAtUtc DATETIME2(3) = NULL,
    @ExpiresAtUtc DATETIME2(3) = NULL,
    @SuspensionReason NVARCHAR(2000) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [ContractorSafety].[PermitToWork]
            ([PermitToWorkId], [TenantId], [SiteId], [Status], [PermitTitle], [PermitType], [WorkLocation], [HazardCount], [RequiresAtmosphericTesting], [AtmosphericTestScore], [AuthorizedByUserId], [IssuedAtUtc], [ExpiresAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@PermitToWorkId, @TenantId, @SiteId, @Status, @PermitTitle, @PermitType, @WorkLocation, @HazardCount, @RequiresAtmosphericTesting, @AtmosphericTestScore, @AuthorizedByUserId, @IssuedAtUtc, @ExpiresAtUtc, @SuspensionReason, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
