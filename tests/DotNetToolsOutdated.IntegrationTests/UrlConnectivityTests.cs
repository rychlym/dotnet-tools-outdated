using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DotNetToolsOutdated.Tests;

public class UrlConnectivityTests
{
    // Simple mapping for common placeholders found in source templates.
    readonly Dictionary<string, string> _placeholderValues = new()
    {
        { "{packageName}", "dotnet-tools-outdated" },
        { "{prereleasePar}", "false" },
        { "{prerelease}", "false" }
    };

    [Fact]
    public async Task AllUrlsAreReachableAsync()
    {
        // Locate repository root (look for 'src' folder)
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "src")))
        {
            dir = dir.Parent;
        }

        var repoRoot = dir?.FullName ?? AppContext.BaseDirectory;
        var srcRoot = Path.Combine(repoRoot, "src");
        Assert.True(Directory.Exists(srcRoot), $"Could not find 'src' directory starting from '{AppContext.BaseDirectory}'");

        // Collect URLs from .cs files under src
        var urlRegex = new Regex(@"https?://[^\s""'()<>]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var files = Directory.EnumerateFiles(srcRoot, "*.cs", SearchOption.AllDirectories);
        var urls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var finalUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in files)
        {
            var text = await File.ReadAllTextAsync(file);
            foreach (Match m in urlRegex.Matches(text))
            {
                if (m.Success)
                {
                    urls.Add(m.Value);
                }
            }
        }

        Assert.NotEmpty(urls); // ensure we actually found something to test

        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(40));
        var failures = new List<string>();

        foreach (var rawUrl in urls.OrderBy(u => u))
        {
            // Replace simple placeholders when present
            var url = rawUrl;
            foreach (var kv in _placeholderValues)
            {
                if (url.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                {
                    url = url.Replace(kv.Key, Uri.EscapeDataString(kv.Value));
                }
            }

            // If url still contains '{' attempt to remove balanced braces to avoid invalid URI
            if (url.Contains("{"))
            {
                // naive: remove anything between braces
                url = Regex.Replace(url, @"\{[^}]*\}", string.Empty);
            }

            finalUrls.Add(url);
            try
            {
                // Request headers only to be light-weight
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cts.Token);

                if (!resp.IsSuccessStatusCode)
                {
                    failures.Add($"{rawUrl} => {(int)resp.StatusCode} {resp.ReasonPhrase} (resolved: {url})");
                }
            }
            catch (Exception ex)
            {
                failures.Add($"{rawUrl} => Exception: {ex.GetType().Name}: {ex.Message} (resolved: {url})");
            }
        }

        if (failures.Count > 0)
        {
            var joined = string.Join(Environment.NewLine, failures);
            Assert.Fail($"Some URL checks failed:{Environment.NewLine}{joined}");
        }
    }
}