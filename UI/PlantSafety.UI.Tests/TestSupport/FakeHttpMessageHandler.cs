// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  Module:         TestSupport
//  File:           FakeHttpMessageHandler.cs
//  Created:        2026-08-09
//  Description:    A minimal, deterministic HttpMessageHandler stub for
//                  bUnit component tests. PlantSafety.UI's *UiService
//                  classes are concrete (not interface-based) and always go
//                  through the single approved PlantSafetyApiClient wrapper
//                  (Section 10), so the real, documented way to test a page
//                  end-to-end without a live PlantSafety.UI.Api process is
//                  to construct a real PlantSafetyApiClient over an
//                  HttpClient whose handler returns canned responses --
//                  never to mock the UiService/ApiClient themselves, which
//                  would test nothing but the mock.
//
//                  Routes are matched by a caller-supplied predicate over
//                  the request's method+path, checked in registration
//                  order; the first match wins. Unmatched requests throw,
//                  surfacing a missing test fixture immediately rather than
//                  hanging or returning a misleading default.
// ============================================================================
using System.Net;

namespace PlantSafety.UI.Tests.TestSupport;

/// <summary>A minimal, deterministic HttpMessageHandler stub returning canned JSON responses by route.</summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly List<(Func<HttpRequestMessage, bool> Match, HttpStatusCode StatusCode, string ResponseJson)> _routes = [];

    /// <summary>Registers a canned JSON response for requests matching the given predicate.</summary>
    public FakeHttpMessageHandler When(Func<HttpRequestMessage, bool> match, string responseJson, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _routes.Add((match, statusCode, responseJson));
        return this;
    }

    /// <summary>Registers a canned JSON response for requests matching the given HTTP method and exact path.</summary>
    public FakeHttpMessageHandler When(HttpMethod method, string path, string responseJson, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        When(request => request.Method == method && request.RequestUri?.AbsolutePath.TrimStart('/') == path.TrimStart('/'), responseJson, statusCode);

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        foreach ((Func<HttpRequestMessage, bool> match, HttpStatusCode statusCode, string responseJson) in _routes)
        {
            if (match(request))
            {
                var response = new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json"),
                };
                return Task.FromResult(response);
            }
        }

        throw new InvalidOperationException($"FakeHttpMessageHandler has no registered route for {request.Method} {request.RequestUri}.");
    }
}
