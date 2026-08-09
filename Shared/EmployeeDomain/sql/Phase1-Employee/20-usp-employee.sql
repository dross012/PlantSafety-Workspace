-- =============================================
-- Shared.EmployeeDomain -- Phase1-Employee
-- Script:        20-usp-employee.sql
-- Purpose:       usp_Employee_Upsert / GetById, usp_EmployeeCertification_Add,
--                usp_EmployeeExposureRecord_Record, usp_EmployeeInjury_Record
--                (updates Employee's running injury summary + appends
--                EmployeeInjuryEvent, one transaction, mirroring
--                EmployeeInjuryProfile.RecordInjury's own semantics),
--                usp_EmployeeTrainingProfile_Replace (replace-wholesale,
--                mirrors FacilityDomain's profile-replace precedent).
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [EmployeeDomain].[usp_Employee_Upsert]
    @EmployeeId                   uniqueidentifier,
    @Name                         nvarchar(200),
    @Role                         nvarchar(30),
    @TrainingProfileUpdatedAtUtc  datetime2(3),
    @SafetyScoreValue             float,
    @SafetyScoreComputedAtUtc     datetime2(3),
    @CreatedAtUtc                 datetime2(3),
    @UpdatedAtUtc                 datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EmployeeId IS NULL OR @EmployeeId = '00000000-0000-0000-0000-000000000000'
            THROW 50001, N'EmployeeId is required and must not be empty.', 1;
        IF @Name IS NULL OR LTRIM(RTRIM(@Name)) = N''
            THROW 50001, N'Name is required.', 1;

        MERGE [EmployeeDomain].[Employee] AS target
        USING (SELECT @EmployeeId AS EmployeeId) AS source
            ON target.[EmployeeId] = source.EmployeeId
        WHEN MATCHED THEN
            UPDATE SET [Name] = @Name, [Role] = @Role,
                [TrainingProfileUpdatedAtUtc] = @TrainingProfileUpdatedAtUtc,
                [SafetyScoreValue] = @SafetyScoreValue, [SafetyScoreComputedAtUtc] = @SafetyScoreComputedAtUtc,
                [UpdatedAtUtc] = @UpdatedAtUtc, [UpdatedUtc] = SYSUTCDATETIME(), [UpdatedBy] = SYSTEM_USER
        WHEN NOT MATCHED THEN
            INSERT ([EmployeeId], [Name], [Role], [TrainingProfileUpdatedAtUtc], [InjuryProfileUpdatedAtUtc],
                    [SafetyScoreValue], [SafetyScoreComputedAtUtc], [CreatedAtUtc], [UpdatedAtUtc])
            VALUES (@EmployeeId, @Name, @Role, @TrainingProfileUpdatedAtUtc, @CreatedAtUtc,
                    @SafetyScoreValue, @SafetyScoreComputedAtUtc, @CreatedAtUtc, @UpdatedAtUtc);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (N'EmployeeDomain.Employee', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'EmployeeId=', @EmployeeId), NULL, N'INFO', OBJECT_NAME(@@PROCID), 0);
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext40 nvarchar(200) = CONCAT(N'Params=EmployeeId=', @EmployeeId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails40 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext40, @ErrorDetails40, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EmployeeDomain].[usp_Employee_GetById]
    @EmployeeId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EmployeeId IS NULL
            THROW 50001, N'EmployeeId is required.', 1;

        SELECT [EmployeeId], [Name], [Role], [TrainingProfileUpdatedAtUtc], [InjuryTotalCount],
               [InjuryLastAtUtc], [InjuryLastDescription], [InjuryProfileUpdatedAtUtc],
               [SafetyScoreValue], [SafetyScoreComputedAtUtc], [CreatedAtUtc], [UpdatedAtUtc]
        FROM [EmployeeDomain].[Employee]
        WHERE [EmployeeId] = @EmployeeId AND ISNULL([IsDeleted], 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext41 nvarchar(200) = CONCAT(N'Params=EmployeeId=', @EmployeeId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails41 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext41, @ErrorDetails41, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EmployeeDomain].[usp_EmployeeTrainingProfile_Replace]
    @EmployeeId          uniqueidentifier,
    @TrainingCodesCsv    nvarchar(max),
    @UpdatedAtUtc        datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EmployeeId IS NULL
            THROW 50001, N'EmployeeId is required.', 1;

        BEGIN TRANSACTION;
        DELETE FROM [EmployeeDomain].[EmployeeTrainingCode] WHERE [EmployeeId] = @EmployeeId;
        INSERT INTO [EmployeeDomain].[EmployeeTrainingCode] ([EmployeeId], [TrainingCode], [UpdatedAtUtc])
        SELECT @EmployeeId, LTRIM(RTRIM(value)), @UpdatedAtUtc
        FROM STRING_SPLIT(@TrainingCodesCsv, ',')
        WHERE LTRIM(RTRIM(value)) <> N'';
        UPDATE [EmployeeDomain].[Employee] SET [TrainingProfileUpdatedAtUtc] = @UpdatedAtUtc WHERE [EmployeeId] = @EmployeeId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorContext42 nvarchar(200) = CONCAT(N'Params=EmployeeId=', @EmployeeId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails42 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext42, @ErrorDetails42, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EmployeeDomain].[usp_EmployeeCertification_Add]
    @EmployeeId          uniqueidentifier,
    @CertificationCode   nvarchar(50),
    @IssuingBody         nvarchar(200),
    @IssuedAtUtc         datetime2(3),
    @ExpiresAtUtc        datetime2(3) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EmployeeId IS NULL
            THROW 50001, N'EmployeeId is required.', 1;

        INSERT INTO [EmployeeDomain].[EmployeeCertification]
            ([EmployeeId], [CertificationCode], [IssuingBody], [IssuedAtUtc], [ExpiresAtUtc])
        VALUES (@EmployeeId, @CertificationCode, @IssuingBody, @IssuedAtUtc, @ExpiresAtUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext43 nvarchar(200) = CONCAT(N'Params=EmployeeId=', @EmployeeId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails43 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext43, @ErrorDetails43, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EmployeeDomain].[usp_EmployeeExposureRecord_Record]
    @EmployeeId       uniqueidentifier,
    @HazardTypeCode   nvarchar(30),
    @Description      nvarchar(500),
    @ExposedAtUtc     datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EmployeeId IS NULL
            THROW 50001, N'EmployeeId is required.', 1;

        INSERT INTO [EmployeeDomain].[EmployeeExposureRecord]
            ([EmployeeId], [HazardTypeCode], [Description], [ExposedAtUtc])
        VALUES (@EmployeeId, @HazardTypeCode, @Description, @ExposedAtUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext44 nvarchar(200) = CONCAT(N'Params=EmployeeId=', @EmployeeId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails44 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext44, @ErrorDetails44, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EmployeeDomain].[usp_EmployeeInjury_Record]
    @EmployeeId    uniqueidentifier,
    @Description   nvarchar(500),
    @OccurredAtUtc datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EmployeeId IS NULL
            THROW 50001, N'EmployeeId is required.', 1;

        BEGIN TRANSACTION;

        INSERT INTO [EmployeeDomain].[EmployeeInjuryEvent] ([EmployeeId], [Description], [OccurredAtUtc])
        VALUES (@EmployeeId, @Description, @OccurredAtUtc);

        UPDATE [EmployeeDomain].[Employee]
        SET [InjuryTotalCount] = [InjuryTotalCount] + 1,
            [InjuryLastAtUtc] = @OccurredAtUtc,
            [InjuryLastDescription] = @Description,
            [InjuryProfileUpdatedAtUtc] = @OccurredAtUtc,
            [UpdatedUtc] = SYSUTCDATETIME(), [UpdatedBy] = SYSTEM_USER
        WHERE [EmployeeId] = @EmployeeId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorContext45 nvarchar(200) = CONCAT(N'Params=EmployeeId=', @EmployeeId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails45 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext45, @ErrorDetails45, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
