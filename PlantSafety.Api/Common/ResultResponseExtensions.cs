// ============================================================================
// File: ResultResponseExtensions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-07-21
// Description: Shared helper mapping the Application layer's canonical Result<T> /
//              ApplicationError outcome onto the Presentation layer's ApiResponse<T>
//              envelope and the matching HTTP status code. Used by every V1 controller
//              that dispatches an ICommand<Result<T>> or IQuery<Result<T>> through ISender,
//              so each controller action does not need to hand-roll the same switch.
// ============================================================================
//
// Onboarding Notes:
// - Application.Common.Results.Result<T> and PlantSafety.SharedKernel.Primitives.Results.Result<T>
//   share a type name. This file fully-qualifies the Application-layer type to avoid ambiguity
//   with the globally-imported SharedKernel namespace.
// - Never used for anything other than translating an already-computed Result<T> to an
//   IActionResult; contains no business logic.
//
// ============================================================================

using PlantSafety.Application.Common.Errors;

namespace PlantSafety.Api.Common;

/// <summary>
/// Extension methods that translate an Application-layer <see cref="PlantSafety.Application.Common.Results.Result{T}"/>
/// into the standard <see cref="ApiResponse{T}"/>-wrapped <see cref="IActionResult"/> shape.
/// </summary>
public static class ResultResponseExtensions
{
    /// <summary>
    /// Maps a completed <see cref="PlantSafety.Application.Common.Results.Result{T}"/> onto the appropriate
    /// <see cref="IActionResult"/>, choosing the HTTP status code from the failure's <see cref="ErrorSeverity"/>
    /// when applicable.
    /// </summary>
    /// <typeparam name="T">The success payload type.</typeparam>
    /// <param name="controller">The controller producing the response.</param>
    /// <param name="result">The Application-layer result to translate.</param>
    /// <param name="correlationId">Correlation identifier echoed into the response envelope.</param>
    /// <returns>A 200 response wrapping the value on success; an error-appropriate status code on failure.</returns>
    public static IActionResult ToActionResult<T>(
        this ControllerBase controller,
        PlantSafety.Application.Common.Results.Result<T> result,
        Guid correlationId)
    {
        if (result.IsSuccess)
            return controller.Ok(ApiResponse<T>.Ok(result.Value, correlationId.ToString()));

        var error = result.Error;
        var response = ApiResponse<T>.Fail(error.Message, correlationId.ToString());

        return error.Severity switch
        {
            ErrorSeverity.NotFound => controller.NotFound(response),
            ErrorSeverity.Validation => controller.UnprocessableEntity(response),
            ErrorSeverity.Authorization => controller.StatusCode(StatusCodes.Status403Forbidden, response),
            ErrorSeverity.Domain => controller.BadRequest(response),
            ErrorSeverity.Critical => controller.StatusCode(StatusCodes.Status500InternalServerError, response),
            _ => controller.StatusCode(StatusCodes.Status500InternalServerError, response)
        };
    }
}
