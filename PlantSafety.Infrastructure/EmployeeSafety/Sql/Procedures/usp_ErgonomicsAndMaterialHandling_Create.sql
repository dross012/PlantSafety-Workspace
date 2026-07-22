-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ErgonomicsAndMaterialHandling_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [EmployeeSafety].[ErgonomicsAndMaterialHandling], backing
--                  IErgonomicsAndMaterialHandlingRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_ErgonomicsAndMaterialHandling_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @ErgonomicsAndMaterialHandlingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @AssessmentTitle NVARCHAR(400),
    @HazardCategory NVARCHAR(100),
    @WorkstationLocation NVARCHAR(400),
    @AssessedByUserId UNIQUEIDENTIFIER,
    @AssessedAtUtc DATETIME2(3) = NULL,
    @RiskFactorCount INT,
    @RiskScore DECIMAL(9,2) = NULL,
    @ControlsDescription NVARCHAR(2000) = NULL,
    @FollowUpDueAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [EmployeeSafety].[ErgonomicsAndMaterialHandling]
            ([ErgonomicsAndMaterialHandlingId], [TenantId], [SiteId], [Status], [AssessmentTitle], [HazardCategory], [WorkstationLocation], [AssessedByUserId], [AssessedAtUtc], [RiskFactorCount], [RiskScore], [ControlsDescription], [FollowUpDueAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@ErgonomicsAndMaterialHandlingId, @TenantId, @SiteId, @Status, @AssessmentTitle, @HazardCategory, @WorkstationLocation, @AssessedByUserId, @AssessedAtUtc, @RiskFactorCount, @RiskScore, @ControlsDescription, @FollowUpDueAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
