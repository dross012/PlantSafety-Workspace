// ============================================================================
//  Project:        Enterprise.ApiGateway
//  Module:         Controllers
//  File:           EnterpriseApiControllerBase.cs
//  Created:        2026-08-08
//  Description:    Phase D.3: common base for every enterprise API gateway
//                  controller. [ApiController] enables automatic model
//                  validation/binding; the shared [Route] prefix anchors
//                  every derived controller under EnterpriseApiRoute.Base
//                  so route composition happens in exactly one place.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Enterprise.ApiGateway.Routing;

namespace Enterprise.ApiGateway.Controllers;

/// <summary>Common base for every enterprise API gateway controller.</summary>
[ApiController]
[Route(EnterpriseApiRoute.Base)]
public abstract class EnterpriseApiControllerBase : ControllerBase
{
    /// <summary>Wraps a non-null result as 200 OK, or 404 Not Found if <paramref name="result"/> is null.</summary>
    protected ActionResult<T> OkOrNotFound<T>(T? result) where T : class =>
        result is null ? NotFound() : Ok(result);
}
