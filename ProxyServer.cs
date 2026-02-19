using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A lightweight HTTP reverse proxy built on <see cref="HttpListener"/>.
/// Forwards incoming HTTP requests to a configured target URL and streams
/// the responses back to the caller. Supports optional rewriting of
/// <c>Host</c>, <c>Referer</c> headers, and in-body target-host references
/// for <c>text/html</c> and <c>application/json</c> responses.
/// </summary>
/// <remarks>
/// <para>
/// The proxy listens on one or more URI prefixes (e.g. <c>http://localhost:5050/</c>)
/// and forwards every request to the <see cref="TargetUrl"/>.
/// </para>
/// <para>
/// <b>Known limitation:</b> The <see cref="ProcessRequest(IAsyncResult)"/> callback is
/// <c>async void</c>. Unhandled exceptions inside the request pipeline will crash the
/// process. Callers should add error handling in a derived class by overriding
/// <see cref="ProcessRequest(HttpListenerContext)"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// using var server = new ProxyServer("https://www.google.com",
///     "http://localhost:5050/", "http://127.0.0.1:5050/");
/// server.Start();
/// Console.ReadKey();
/// server.Stop();
/// </code>
/// </example>
public class ProxyServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly int _targetPort;
    private readonly string _targetHost;
    private static readonly HttpClient _client = new HttpClient();

    /// <summary>
    /// Initializes a new <see cref="ProxyServer"/> that forwards requests to <paramref name="targetUrl"/>.
    /// </summary>
    /// <param name="targetUrl">The absolute URL of the upstream server to forward requests to.</param>
    /// <param name="prefixes">
    /// One or more URI prefixes on which the proxy will listen
    /// (e.g. <c>"http://localhost:5050/"</c>). Each prefix must end with a trailing slash.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="targetUrl"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="prefixes"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="prefixes"/> is empty.</exception>
    public ProxyServer(string targetUrl, params string[] prefixes)
        : this(new Uri(targetUrl), prefixes)
    {
    }

    /// <inheritdoc cref="ProxyServer(string, string[])"/>
    public ProxyServer(Uri targetUrl, params string[] prefixes)
    {
        if (targetUrl == null)
            throw new ArgumentNullException(nameof(targetUrl));

        if (prefixes == null)
            throw new ArgumentNullException(nameof(prefixes));

        if (prefixes.Length == 0)
            throw new ArgumentException(null, nameof(prefixes));

        RewriteTargetInText = true;
        RewriteHost = true;
        RewriteReferer = true;
        TargetUrl = targetUrl;
        _targetHost = targetUrl.Host;
        _targetPort = targetUrl.Port;
        Prefixes = prefixes;

        _listener = new HttpListener();
        foreach (var prefix in prefixes)
        {
            _listener.Prefixes.Add(prefix);
        }
    }

    /// <summary>Gets the upstream server URL that requests are forwarded to.</summary>
    public Uri TargetUrl { get; }

    /// <summary>Gets the URI prefixes the proxy is listening on.</summary>
    public string[] Prefixes { get; }

    /// <summary>
    /// Gets or sets whether target-host references inside <c>text/html</c> and
    /// <c>application/json</c> response bodies are rewritten to point back through
    /// the proxy. Defaults to <c>true</c>.
    /// </summary>
    public bool RewriteTargetInText { get; set; }

    /// <summary>
    /// Gets or sets whether the <c>Host</c> header in forwarded requests is
    /// rewritten to match <see cref="TargetUrl"/>. Defaults to <c>true</c>.
    /// </summary>
    public bool RewriteHost { get; set; }

    /// <summary>
    /// Gets or sets whether the <c>Referer</c> header is rewritten to target the
    /// upstream host. Defaults to <c>true</c>. May have a performance impact on
    /// high-traffic proxies.
    /// </summary>
    public bool RewriteReferer { get; set; }

    /// <summary>
    /// Starts listening for incoming HTTP requests on the configured <see cref="Prefixes"/>.
    /// Each received request is forwarded asynchronously to <see cref="TargetUrl"/>.
    /// </summary>
    public void Start()
    {
        _listener.Start();
        _listener.BeginGetContext(ProcessRequest, null);
    }

    /// <summary>
    /// APM callback invoked by <see cref="HttpListener.BeginGetContext"/>.
    /// Completes the pending context, immediately begins listening for the next
    /// request, and processes the current one asynchronously.
    /// </summary>
    /// <remarks>
    /// This method is <c>async void</c> because it is an APM callback.
    /// Unhandled exceptions will crash the process — override
    /// <see cref="ProcessRequest(HttpListenerContext)"/> and add a try/catch
    /// to handle errors gracefully.
    /// </remarks>
    private async void ProcessRequest(IAsyncResult result)
    {
        if (!_listener.IsListening)
            return;

        var ctx = _listener.EndGetContext(result);
        _listener.BeginGetContext(ProcessRequest, null);
        await ProcessRequest(ctx).ConfigureAwait(false);
    }

    /// <summary>
    /// Forwards a single HTTP request to the <see cref="TargetUrl"/> and streams
    /// the response back to the original caller.
    /// </summary>
    /// <param name="context">The <see cref="HttpListenerContext"/> representing the incoming request.</param>
    /// <returns>A task that completes when the response has been fully written.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is <c>null</c>.</exception>
    /// <remarks>
    /// Override this method to customize request/response handling or to add
    /// error handling around the forwarding pipeline.
    /// </remarks>
    protected virtual async Task ProcessRequest(HttpListenerContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var url = TargetUrl.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
        using (var msg = new HttpRequestMessage(new HttpMethod(context.Request.HttpMethod), url + context.Request.RawUrl))
        {
            msg.Version = context.Request.ProtocolVersion;

            if (context.Request.HasEntityBody)
            {
                msg.Content = new StreamContent(context.Request.InputStream); // disposed with msg
            }

            string host = null;
            foreach (string headerName in context.Request.Headers)
            {
                var headerValue = context.Request.Headers[headerName];
                if (headerName == "Content-Length" && headerValue == "0") // useless plus don't send if we have no entity body
                    continue;

                bool contentHeader = false;
                switch (headerName)
                {
                    // some headers go to content...
                    case "Allow":
                    case "Content-Disposition":
                    case "Content-Encoding":
                    case "Content-Language":
                    case "Content-Length":
                    case "Content-Location":
                    case "Content-MD5":
                    case "Content-Range":
                    case "Content-Type":
                    case "Expires":
                    case "Last-Modified":
                        contentHeader = true;
                        break;

                    case "Referer":
                        if (RewriteReferer && Uri.TryCreate(headerValue, UriKind.Absolute, out var referer)) // if relative, don't handle
                        {
                            var builder = new UriBuilder(referer);
                            builder.Host = TargetUrl.Host;
                            builder.Port = TargetUrl.Port;
                            headerValue = builder.ToString();
                        }
                        break;

                    case "Host":
                        host = headerValue;
                        if (RewriteHost)
                        {
                            headerValue = TargetUrl.Host + ":" + TargetUrl.Port;
                        }
                        break;
                }

                if (contentHeader)
                {
                    msg.Content.Headers.Add(headerName, headerValue);
                }
                else
                {
                    msg.Headers.Add(headerName, headerValue);
                }
            }

            using (var response = await _client.SendAsync(msg).ConfigureAwait(false))
            {
                using (var os = context.Response.OutputStream)
                {
                    context.Response.ProtocolVersion = response.Version;
                    context.Response.StatusCode = (int)response.StatusCode;
                    context.Response.StatusDescription = response.ReasonPhrase;

                    foreach (var header in response.Headers)
                    {
                        context.Response.Headers.Add(header.Key, string.Join(", ", header.Value));
                    }

                    foreach (var header in response.Content.Headers)
                    {
                        if (header.Key == "Content-Length") // this will be set automatically at dispose time
                            continue;

                        context.Response.Headers.Add(header.Key, string.Join(", ", header.Value));
                    }

                    var ct = context.Response.ContentType;
                    if (RewriteTargetInText && host != null && ct != null &&
                        (ct.IndexOf("text/html", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        ct.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        using (var ms = new MemoryStream())
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                            {
                                await stream.CopyToAsync(ms).ConfigureAwait(false);
                                var enc = context.Response.ContentEncoding ?? Encoding.UTF8;
                                var html = enc.GetString(ms.ToArray());
                                if (TryReplace(html, "//" + _targetHost + ":" + _targetPort + "/", "//" + host + "/", out var replaced))
                                {
                                    var bytes = enc.GetBytes(replaced);
                                    using (var ms2 = new MemoryStream(bytes))
                                    {
                                        ms2.Position = 0;
                                        await ms2.CopyToAsync(context.Response.OutputStream).ConfigureAwait(false);
                                    }
                                }
                                else
                                {
                                    ms.Position = 0;
                                    await ms.CopyToAsync(context.Response.OutputStream).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                    else
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            await stream.CopyToAsync(context.Response.OutputStream).ConfigureAwait(false);
                        }
                    }
                }
            }
        }
    }

    /// <summary>Stops listening for incoming requests.</summary>
    public void Stop() => _listener.Stop();

    /// <summary>Returns a string showing the proxy mapping (prefixes => target).</summary>
    public override string ToString() => string.Join(", ", Prefixes) + " => " + TargetUrl;

    /// <summary>Releases the underlying <see cref="HttpListener"/> resources.</summary>
    public void Dispose() => ((IDisposable)_listener)?.Dispose();

    /// <summary>
    /// Attempts to replace all occurrences of <paramref name="oldValue"/> with
    /// <paramref name="newValue"/> in <paramref name="input"/>, reporting whether
    /// any replacement was actually made.
    /// </summary>
    /// <param name="input">The source string to search.</param>
    /// <param name="oldValue">The substring to find.</param>
    /// <param name="newValue">The replacement string.</param>
    /// <param name="result">
    /// When this method returns, contains the modified string if a replacement
    /// occurred; otherwise, the original <paramref name="input"/>.
    /// </param>
    /// <returns><c>true</c> if at least one replacement was made; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// Uses a character-by-character scan. Does <b>not</b> handle overlapping
    /// matches correctly (e.g. searching for <c>"aab"</c> in <c>"aaab"</c> will
    /// miss the match). For most proxy rewriting scenarios this is acceptable
    /// because the search pattern contains <c>://</c> which is unlikely to overlap.
    /// </remarks>
    private static bool TryReplace(string input, string oldValue, string newValue, out string result)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(oldValue))
        {
            result = input;
            return false;
        }

        var oldLen = oldValue.Length;
        var sb = new StringBuilder(input.Length);
        bool changed = false;
        var offset = 0;
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];

            if (offset > 0)
            {
                if (c == oldValue[offset])
                {
                    offset++;
                    if (oldLen == offset)
                    {
                        changed = true;
                        sb.Append(newValue);
                        offset = 0;
                    }
                    continue;
                }

                for (int j = 0; j < offset; j++)
                {
                    sb.Append(input[i - offset + j]);
                }

                sb.Append(c);
                offset = 0;
            }
            else
            {
                if (c == oldValue[0])
                {
                    if (oldLen == 1)
                    {
                        changed = true;
                        sb.Append(newValue);
                    }
                    else
                    {
                        offset = 1;
                    }
                    continue;
                }

                sb.Append(c);
            }
        }

        if (changed)
        {
            result = sb.ToString();
            return true;
        }

        result = input;
        return false;
    }
}