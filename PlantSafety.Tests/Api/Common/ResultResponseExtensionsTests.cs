// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Api.Common
//  Layer:          Tests (Unit)
//  File:           ResultResponseExtensionsTests.cs
//  Created:        2026-07-29
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ResultResponseExtensions.ToActionResult -- the single chokepoint
//                  that maps every module's ApplicationError.Severity onto an HTTP status code.
//                  Added while verifying the RiskManagement NotFound-severity fix: a missing Risk
//                  must surface as 404, not the previous accidental 500. Covers the mapping
//                  generically (every ErrorSeverity value) rather than just the Risk case, since
//                  this is shared infrastructure, not RiskManagement-specific.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlantSafety.Api.Common;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Tests.Api.Common;

public sealed class ResultResponseExtensionsTests
{
    private static readonly ControllerBase Controller = new EmptyController();

    [Fact]
    public void ToActionResult_WithSuccess_Returns200Ok()
    {
        var result = Result<string>.Success("value");

        var actionResult = Controller.ToActionResult(result, Guid.NewGuid());

        actionResult.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void ToActionResult_WithNotFoundSeverity_Returns404NotFound()
    {
        var result = Result<string>.Failure(new ApplicationError(
            "RiskManagement.Risk.NotFound", "The requested risk could not be found.", Guid.NewGuid(), ErrorSeverity.NotFound));

        var actionResult = Controller.ToActionResult(result, Guid.NewGuid());

        actionResult.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void ToActionResult_WithDefaultErrorSeverity_Returns500InternalServerError()
    {
        // ApplicationError's default severity (when omitted, as the RiskManagement command
        // handlers previously did for their NotFound errors) is ErrorSeverity.Error, not NotFound.
        var result = Result<string>.Failure(new ApplicationError(
            "RiskManagement.Risk.NotFound", "The requested risk could not be found.", Guid.NewGuid()));

        var actionResult = Controller.ToActionResult(result, Guid.NewGuid());

        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void ToActionResult_WithValidationSeverity_Returns422UnprocessableEntity()
    {
        var result = Result<string>.Failure(new ApplicationError(
            "Platform.Validation.Failed", "Invalid input.", Guid.NewGuid(), ErrorSeverity.Validation));

        var actionResult = Controller.ToActionResult(result, Guid.NewGuid());

        var objectResult = actionResult.Should().BeOfType<UnprocessableEntityObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    public void ToActionResult_WithAuthorizationSeverity_Returns403Forbidden()
    {
        var result = Result<string>.Failure(new ApplicationError(
            "Platform.Auth.Forbidden", "Not permitted.", Guid.NewGuid(), ErrorSeverity.Authorization));

        var actionResult = Controller.ToActionResult(result, Guid.NewGuid());

        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void ToActionResult_WithDomainSeverity_Returns400BadRequest()
    {
        var result = Result<string>.Failure(new ApplicationError(
            "RiskManagement.Risk.InvalidState", "Invalid state transition.", Guid.NewGuid(), ErrorSeverity.Domain));

        var actionResult = Controller.ToActionResult(result, Guid.NewGuid());

        actionResult.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>Minimal concrete <see cref="ControllerBase"/> for exercising the extension method in isolation.</summary>
    private sealed class EmptyController : ControllerBase;
}
