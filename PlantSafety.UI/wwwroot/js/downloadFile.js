// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Minimal browser-download helper invoked via IJSRuntime from
//   AuditViewerDashboardPage's "Export" button. Builds a Blob from base64-encoded
//   bytes fetched by the typed API client (auth headers already applied server-side
//   by the HttpClient handler pipeline) and triggers a standard <a download> click.
//   No other module currently needs a browser download, so this is intentionally the
//   first and only JS interop file in PlantSafety.UI -- see the CSP script-src 'self'
//   rule in wwwroot/index.html, which this file satisfies.
// ============================================================
window.plantSafetyDownloadFile = (fileName, base64Content, contentType) => {
    const byteCharacters = atob(base64Content);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: contentType });
    const url = URL.createObjectURL(blob);

    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    URL.revokeObjectURL(url);
};
