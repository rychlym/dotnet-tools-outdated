using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static int Main(string[] args)
    {
        var repoPath = Directory.GetCurrentDirectory();
        var days = 30;
        if (args.Length >= 1 && (args[0] == "--help" || args[0] == "-h"))
        {
            Console.WriteLine("Usage: dotnet run -- [--days N] [repoPath]");
            return 0;
        }
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--days" && i + 1 < args.Length && int.TryParse(args[i + 1], out var d)) { days = d; i++; }
            else repoPath = args[i];
        }

        var gitLog = RunGit(repoPath, $"log --pretty=format:%h;%ad;%s --date=short --since=\"{days}.days\" --no-merges");
        if (gitLog == null)
        {
            Console.Error.WriteLine("Failed to run git. Ensure git is available and this is a git repo.");
            return 2;
        }

        var lines = gitLog.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var versionRegex = new Regex(@"\b(v?\d+\.\d+\.\d+)\b", RegexOptions.IgnoreCase);
        var keywordRegex = new Regex(@"\b(version|bump)\b", RegexOptions.IgnoreCase);

        var grouped = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            // format: <hash>;<date>;<subject>
            var parts = line.Split(new[] { ';' }, 3);
            if (parts.Length < 3) continue;
            var hash = parts[0];
            var date = parts[1];
            var subject = parts[2];

            var m = versionRegex.Match(subject);
            var key = m.Success ? m.Groups[1].Value : (keywordRegex.IsMatch(subject) ? "Unreleased" : null);
            if (key == null) continue; // ignore unrelated commits

            if (!grouped.TryGetValue(key, out var list)) { list = new List<string>(); grouped[key] = list; }
            list.Add($"- {date} {hash} {subject}");
        }

        if (grouped.Count == 0)
        {
            Console.WriteLine("No version-related commits found in the specified time window.");
            return 0;
        }

        // Build changelog content; newest versions first
        var sb = new StringBuilder();
        sb.AppendLine("# Changelog (auto-generated)");
        sb.AppendLine($"_Generated: {DateTime.UtcNow:yyyy-MM-dd}_");
        sb.AppendLine();

        foreach (var kv in grouped.OrderByDescending(k => ParseVersionSortKey(k.Key)))
        {
            sb.AppendLine($"## [{kv.Key}]");
            foreach (var item in kv.Value)
                sb.AppendLine(item);
            sb.AppendLine();
        }

        var changelogPath = Path.Combine(repoPath, "CHANGELOG.md");
        string existing = File.Exists(changelogPath) ? File.ReadAllText(changelogPath) : string.Empty;
        var newContent = sb.ToString() + Environment.NewLine + existing;
        File.WriteAllText(changelogPath, newContent, Encoding.UTF8);

        Console.WriteLine($"Prepended {grouped.Count} changelog section(s) to {changelogPath}");
        return 0;
    }

    static string? RunGit(string workingDir, string arguments)
    {
        try
        {
            var psi = new ProcessStartInfo("git", arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDir
            };
            using var p = Process.Start(psi);
            if (p == null) return null;
            var outp = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return outp;
        }
        catch
        {
            return null;
        }
    }

    static double ParseVersionSortKey(string key)
    {
        // Give higher priority to semantic versions; Unreleased gets lowest numeric value (so appears first after ordering desc)
        if (key.Equals("Unreleased", StringComparison.OrdinalIgnoreCase)) return double.MaxValue;
        var m = Regex.Match(key, @"v?(\d+)\.(\d+)\.(\d+)");
        if (!m.Success) return double.MinValue;
        if (int.TryParse(m.Groups[1].Value, out var a) && int.TryParse(m.Groups[2].Value, out var b) && int.TryParse(m.Groups[3].Value, out var c))
        {
            return a * 1_000_000 + b * 1_000 + c;
        }
        return double.MinValue;
    }
}