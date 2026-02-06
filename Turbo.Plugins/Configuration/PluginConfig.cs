using System;
using System.IO;

namespace Turbo.Plugins.Configuration;

public class PluginConfig
{
    public const string SECTION_NAME = "Turbo:Plugin";

    public string PluginFolderPath { get; init; } =
        Path.Combine(AppContext.BaseDirectory, "plugins");

    public bool EnableHotReload { get; init; } = false;

    public int HotReloadDebounceMs { get; init; } = 500;

    public string[] HotReloadGlobs { get; init; } = ["manifest.json", "*.dll", "*.pdb", "*.deps.json"];

    public HotReloadSessionRefreshMode HotReloadSessionRefresh { get; init; } =
        HotReloadSessionRefreshMode.RestoreClient;

    public bool AutoBuildOnSourceChange { get; init; } = false;

    public int AutoBuildDebounceMs { get; init; } = 900;

    public string[] AutoBuildProjectPaths { get; init; } = [];
}
