using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace Toolbelt.Blazor.WebAssembly.ExtensibleGateway.UserSecretsExtension;

public class UserSecretsExtensionStartupFilter : IStartupFilter
{
    /// <summary>
    /// The request headers that would make the gateway answer with something other than the plain,
    /// complete and uncompressed content of the file, which is what this extension has to merge into.
    /// </summary>
    private static readonly string[] _HeadersToStrip = [
        "Accept-Encoding",      // the gateway serves a pre-compressed variant when this allows it
        "Range", "If-Range",    // a partial response cannot be merged
        "If-None-Match", "If-Modified-Since", "If-Match", "If-Unmodified-Since" // a 304 has no body
    ];

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        // The user secrets ID of the project being served is handed over by this extension's
        // MSBuild targets, through the response file the gateway reads at startup.
        var userSecretsId = Environment.GetEnvironmentVariable("DOTNET_USER_SECRETS_ID");
        if (string.IsNullOrEmpty(userSecretsId)) return app => next(app);

        var secretsJsonPath = GetSecretsJsonPath(userSecretsId);
        if (secretsJsonPath is null) return app => next(app);

        return app =>
        {
            app.Use((context, nextMiddleware) => MergeSecretsIntoAppSettingsResponse(context, nextMiddleware, secretsJsonPath));

            next(app);
        };
    }

    private static string? GetSecretsJsonPath(string userSecretsId)
    {
        try { return PathHelper.GetSecretsPathFromSecretsId(userSecretsId); }
        catch (InvalidOperationException) { return null; }
    }

    /// <summary>
    /// Tells whether the request asks for one of the configuration files of a Blazor WebAssembly
    /// standalone app, such as "appsettings.json" or "appsettings.Development.json". Only the last
    /// segment is looked at, because this middleware sits in front of "UsePathBase" and therefore
    /// still sees the path prefix the gateway is hosting the app under.
    /// </summary>
    internal static bool IsAppSettingsJsonPath(PathString path)
    {
        var value = path.Value;
        if (string.IsNullOrEmpty(value)) return false;

        var fileName = value.AsSpan(value.LastIndexOf('/') + 1);
        return fileName.StartsWith("appsettings.", StringComparison.OrdinalIgnoreCase) &&
               fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task MergeSecretsIntoAppSettingsResponse(HttpContext context, Func<Task> nextMiddleware, string secretsJsonPath)
    {
        if (!HttpMethods.IsGet(context.Request.Method) ||
            !IsAppSettingsJsonPath(context.Request.Path) ||
            !File.Exists(secretsJsonPath))
        {
            await nextMiddleware();
            return;
        }

        foreach (var header in _HeadersToStrip) context.Request.Headers.Remove(header);

        // Capture what the gateway would have sent, so that it can be merged before it goes out.
        var originalBody = context.Response.Body;
        using var capturedBody = new MemoryStream();
        context.Response.Body = capturedBody;
        try
        {
            await nextMiddleware();
        }
        finally
        {
            context.Response.Body = originalBody;
        }

        var capturedBytes = capturedBody.ToArray();

        // Anything but a plain "200 OK" carrying the file content is passed through untouched. A
        // request for a configuration file that the app does not have, for example, answers 404.
        var responseBytes = context.Response.StatusCode == StatusCodes.Status200OK
            ? TryMergeSecrets(capturedBytes, await File.ReadAllTextAsync(secretsJsonPath)) ?? capturedBytes
            : capturedBytes;

        if (!ReferenceEquals(responseBytes, capturedBytes))
        {
            // The body is no longer the content of the file on disk, so the validators that
            // identify that file must not be sent along with it.
            context.Response.Headers.Remove("ETag");
            context.Response.Headers.Remove("Last-Modified");
        }

        context.Response.ContentLength = responseBytes.Length;
        await originalBody.WriteAsync(responseBytes);
    }

    private static byte[]? TryMergeSecrets(byte[] appSettingsBytes, string secretsJson)
    {
        try
        {
            var appSettingsJson = Encoding.UTF8.GetString(appSettingsBytes);
            return Encoding.UTF8.GetBytes(JsonMerger.Merge(appSettingsJson, secretsJson));
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }
}
