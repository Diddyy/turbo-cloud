using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Turbo.Plugins.Configuration;

namespace Turbo.Plugins;

public sealed class PluginAutoBuildService(
    IOptions<PluginConfig> config,
    ILogger<PluginAutoBuildService> logger
) : IHostedService, IDisposable
{
    private readonly PluginConfig _config = config.Value;
    private readonly ILogger<PluginAutoBuildService> _logger = logger;
    private readonly SemaphoreSlim _buildGate = new(1, 1);
    private readonly CancellationTokenSource _cts = new();
    private readonly List<FileSystemWatcher> _watchers = [];
    private readonly object _stateLock = new();
    private readonly HashSet<string> _pendingProjects = new(StringComparer.OrdinalIgnoreCase);

    private Timer? _debounceTimer;
    private int _shutdownStarted;

    public Task StartAsync(CancellationToken ct)
    {
        if (!_config.AutoBuildOnSourceChange || _config.AutoBuildProjectPaths.Length == 0)
            return Task.CompletedTask;

        var projects = _config.AutoBuildProjectPaths
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(Path.GetFullPath)
            .Where(File.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (projects.Length == 0)
            return Task.CompletedTask;

        _debounceTimer = new Timer(
            _ => _ = Task.Run(ProcessBuildQueueAsync, _cts.Token),
            null,
            Timeout.Infinite,
            Timeout.Infinite
        );

        foreach (var projectPath in projects)
        {
            var dir = Path.GetDirectoryName(projectPath)!;
            var watcher = new FileSystemWatcher(dir)
            {
                IncludeSubdirectories = true,
                NotifyFilter =
                    NotifyFilters.FileName
                    | NotifyFilters.DirectoryName
                    | NotifyFilters.LastWrite
                    | NotifyFilters.CreationTime
                    | NotifyFilters.Size,
                EnableRaisingEvents = true,
            };

            watcher.Changed += (_, e) => OnFsEvent(projectPath, e.FullPath);
            watcher.Created += (_, e) => OnFsEvent(projectPath, e.FullPath);
            watcher.Deleted += (_, e) => OnFsEvent(projectPath, e.FullPath);
            watcher.Renamed += (_, e) => OnFsEvent(projectPath, e.FullPath);
            watcher.Error += (_, e) =>
                _logger.LogWarning(e.GetException(), "Auto-build file watcher error for {Project}", projectPath);

            _watchers.Add(watcher);
        }

        _logger.LogInformation(
            "Plugin auto-build is enabled for {Count} project(s).",
            projects.Length
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct)
    {
        Shutdown();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Shutdown();
        _buildGate.Dispose();
        _cts.Dispose();
    }

    private void OnFsEvent(string projectPath, string changedPath)
    {
        if (Volatile.Read(ref _shutdownStarted) == 1)
            return;

        if (!ShouldTriggerBuild(changedPath))
            return;

        lock (_stateLock)
        {
            _pendingProjects.Add(projectPath);
            _debounceTimer?.Change(
                Math.Max(200, _config.AutoBuildDebounceMs),
                Timeout.Infinite
            );
        }
    }

    private async Task ProcessBuildQueueAsync()
    {
        if (_cts.IsCancellationRequested)
            return;

        string[] projects;

        lock (_stateLock)
        {
            if (_pendingProjects.Count == 0)
                return;

            projects = [.. _pendingProjects];
            _pendingProjects.Clear();
        }

        try
        {
            await _buildGate.WaitAsync(_cts.Token).ConfigureAwait(false);

            foreach (var project in projects)
            {
                if (_cts.IsCancellationRequested)
                    return;

                await BuildProjectAsync(project, _cts.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // host is stopping
        }
        finally
        {
            _buildGate.Release();
        }
    }

    private async Task BuildProjectAsync(string projectPath, CancellationToken ct)
    {
        var workingDir = Path.GetDirectoryName(projectPath)!;

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{projectPath}\" -c Debug --nologo",
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        _logger.LogInformation("Auto-building plugin project: {Project}", projectPath);

        using var process = new Process { StartInfo = psi };
        var stdOut = new StringBuilder();
        var stdErr = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                stdOut.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                stdErr.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync(ct).ConfigureAwait(false);

        if (process.ExitCode == 0)
        {
            _logger.LogInformation("Auto-build succeeded for {Project}", projectPath);
            return;
        }

        _logger.LogWarning(
            "Auto-build failed for {Project} (exit {Code}).\n{StdOut}\n{StdErr}",
            projectPath,
            process.ExitCode,
            stdOut.ToString(),
            stdErr.ToString()
        );
    }

    private static bool ShouldTriggerBuild(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
            return false;

        var segments = fullPath.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);

        if (
            segments.Any(s => string.Equals(s, "bin", StringComparison.OrdinalIgnoreCase))
            || segments.Any(s => string.Equals(s, "obj", StringComparison.OrdinalIgnoreCase))
            || segments.Any(s => string.Equals(s, ".git", StringComparison.OrdinalIgnoreCase))
        )
            return false;

        var ext = Path.GetExtension(fullPath);
        if (string.IsNullOrWhiteSpace(ext))
            return false;

        return
            ext.Equals(".cs", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".csproj", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".json", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".props", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".targets", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".editorconfig", StringComparison.OrdinalIgnoreCase);
    }

    private void DisposeWatchers()
    {
        foreach (var watcher in _watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        _watchers.Clear();
    }

    private void Shutdown()
    {
        if (Interlocked.Exchange(ref _shutdownStarted, 1) == 1)
            return;

        DisposeWatchers();
        _debounceTimer?.Dispose();
        _debounceTimer = null;
        _cts.Cancel();
    }
}
