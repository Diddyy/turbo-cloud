using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Turbo.Contracts.Plugins;
using Turbo.Plugins.Configuration;

namespace Turbo.Plugins.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTurboPlugins(
        this IServiceCollection services,
        HostApplicationBuilder builder
    )
    {
        var pluginSection = builder.Configuration.GetSection(PluginConfig.SECTION_NAME);

        services.Configure<PluginConfig>(pluginSection);

        services.AddSingleton<PluginManager>();
        services.AddHostedService<PluginBootstrapper>();

        var pluginConfig = pluginSection.Get<PluginConfig>() ?? new PluginConfig();

        if (builder.Environment.IsDevelopment() && pluginConfig.EnableHotReload)
            services.AddHostedService<PluginHotReloadService>();

        if (
            builder.Environment.IsDevelopment()
            && pluginConfig.AutoBuildOnSourceChange
            && pluginConfig.AutoBuildProjectPaths.Length > 0
        )
            services.AddHostedService<PluginAutoBuildService>();

        return services;
    }

    public static IServiceCollection AddHostPlugin<TModule>(
        this IServiceCollection services,
        HostApplicationBuilder builder
    )
        where TModule : class, IHostPluginModule, new()
    {
        var module = new TModule();

        module.ConfigureServices(services, builder);

        services.AddSingleton<IHostPluginModule>(module);

        return services;
    }
}
