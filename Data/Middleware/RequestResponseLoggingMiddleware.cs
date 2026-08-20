using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http.Features;

namespace Data.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        // Ensure buffering is enabled with a generous limit (10 MB)
        context.Request.EnableBuffering(bufferThreshold: 1024 * 30, bufferLimit: 1024 * 1024 * 10);

        // Log receipt immediately, before the body is buffered/read, so slow or
        // stalled uploads are still visible in the console.
        var hasRoblosecurityCookie = context.Request.Cookies.ContainsKey(".ROBLOSECURITY");
        Console.WriteLine(
            $"info: Custom.RequestLogger[0]      Received {context.Request.Method} {context.Request.Path}{context.Request.QueryString} " +
            $"Host={context.Request.Host} Content-Length={context.Request.ContentLength} " +
            $"CF-Connecting-IP=[Redacted] Cookie={hasRoblosecurityCookie}");

        var requestInfo = await BuildRequestInfo(context.Request);

        // Capture the response by swapping the body stream.
        var originalBody = context.Response.Body;
        var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        try
        {
            try
            {
                await _next(context);
                sw.Stop();
            }
            finally
            {
                // Read response body safely. A downstream exception (handled by the
                // developer-exception page middleware, etc.) may have already closed
                // or replaced context.Response.Body, so guard each step.
                string responseText = string.Empty;
                if (responseBuffer.CanSeek && responseBuffer.Length > 0)
                {
                    try
                    {
                        responseBuffer.Seek(0, SeekOrigin.Begin);
                        responseText = await new StreamReader(responseBuffer, Encoding.UTF8, leaveOpen: true).ReadToEndAsync();
                        responseBuffer.Seek(0, SeekOrigin.Begin);
                    }
                    catch (ObjectDisposedException) { /* stream was torn down mid-flight */ }
                }

                // Always restore the original body before logging so a logging failure
                // can't take down the actual response.
                context.Response.Body = originalBody;

                try
                {
                    PrintLog(requestInfo, context, responseText, sw.Elapsed);
                }
                catch
                {
                    // Logging must never fail the request.
                }

                // Copy the buffered response back to the real body. Guard against the
                // case where the original stream is already closed by downstream code.
                if (responseBuffer.CanSeek && responseBuffer.Length > 0)
                {
                    try
                    {
                        await responseBuffer.CopyToAsync(originalBody);
                    }
                    catch (ObjectDisposedException) { /* downstream already flushed */ }
                    catch (IOException) { /* connection aborted by client */ }
                }
            }
        }
        finally
        {
            responseBuffer.Dispose();
        }
    }

    private static async Task<string> BuildRequestInfo(HttpRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine("info: Custom.RequestLogger[1]");
        sb.AppendLine("      Request:");
        sb.AppendLine($"      Protocol: {request.Protocol}");
        sb.AppendLine($"      Method: {request.Method}");
        sb.AppendLine($"      Scheme: {request.Scheme}");
        sb.AppendLine($"      PathBase: {request.PathBase}");
        sb.AppendLine($"      Path: {request.Path}");
        sb.AppendLine($"      QueryString: {request.QueryString}");
        var rawTarget = request.HttpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;
        if (!string.IsNullOrEmpty(rawTarget))
        {
            sb.AppendLine($"      RawTarget: {rawTarget}");
        }
        sb.AppendLine($"      Host: {request.Host}");
        sb.AppendLine($"      ClientCert: {request.HttpContext.Connection.ClientCertificate != null}");

        // Headers
        foreach (var header in request.Headers)
        {
            if (IsClientIpHeader(header.Key))
            {
                sb.AppendLine($"      {header.Key}: [Redacted]");
            }
            else
            {
                sb.AppendLine($"      {header.Key}: {Sanitize(header.Value)}");
            }
        }

        // Body
        string bodyText = string.Empty;
        try
        {
            // Regardless of Content-Length, attempt to read the buffered body (covers chunked/proxy cases)
            if (request.Body.CanSeek)
            {
                request.Body.Seek(0, SeekOrigin.Begin);
            }
            using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            bodyText = await reader.ReadToEndAsync();
            if (request.Body.CanSeek)
            {
                request.Body.Seek(0, SeekOrigin.Begin);
            }
        }
        catch
        {
            // Ignore body read errors; continue logging other parts
        }

        if (!string.IsNullOrWhiteSpace(bodyText))
        {
            sb.AppendLine("      Body:");
            var display = bodyText.Length > 300 ? bodyText.Substring(0, 300) + "... [truncated]" : bodyText;
            sb.AppendLine("      " + display.Replace("\n", "\n      "));
        }

        // Form fields (if applicable)
        try
        {
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();
                if (form.Count > 0)
                {
                    sb.AppendLine("      Form:");
                    foreach (var kv in form)
                    {
                        sb.AppendLine($"      {kv.Key}: {Sanitize(kv.Value)}");
                    }
                }
            }
        }
        catch
        {
            // Ignore form parsing errors
        }

        return sb.ToString();
    }

    private static void PrintLog(string requestInfo, HttpContext context, string responseText, TimeSpan elapsed)
    {
        var sb = new StringBuilder();
        sb.Append(requestInfo);
        sb.AppendLine("info: Custom.RequestLogger[2]");
        sb.AppendLine("      Response:");
        sb.AppendLine($"      StatusCode: {context.Response.StatusCode}");

        foreach (var header in context.Response.Headers)
        {
            sb.AppendLine($"      {header.Key}: {Sanitize(header.Value)}");
        }

        if (!string.IsNullOrEmpty(responseText))
        {
            var body = responseText.Length > 300 ? responseText.Substring(0, 300) + "... [truncated]" : responseText;
            sb.AppendLine("      Body:");
            sb.AppendLine("      " + body.Replace("\n", "\n      "));
        }
        else
        {
            sb.AppendLine("      Body:");
            sb.AppendLine("      (empty)");
        }

        sb.AppendLine($"      Duration: {elapsed.TotalMilliseconds:n0} ms");

        Console.WriteLine(sb.ToString());
    }

    private static string Sanitize(StringValues value)
    {
        // Keep as-is but allow easy redaction extension in future
        return value.ToString();
    }

    private static bool IsClientIpHeader(string key)
    {
        return key.Equals("X-Forwarded-For", StringComparison.OrdinalIgnoreCase)
            || key.Equals("X-Real-IP", StringComparison.OrdinalIgnoreCase)
            || key.Equals("CF-Connecting-IP", StringComparison.OrdinalIgnoreCase)
            || key.Equals("Cf-Connecting-Ip", StringComparison.OrdinalIgnoreCase)
            || key.Equals("True-Client-IP", StringComparison.OrdinalIgnoreCase)
            || key.Equals("X-Client-IP", StringComparison.OrdinalIgnoreCase)
            || key.Equals("X-Cluster-Client-IP", StringComparison.OrdinalIgnoreCase);
    }
}
