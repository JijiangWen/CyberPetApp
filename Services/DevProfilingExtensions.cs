#if DEBUG
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CyberPetApp.Services;

internal static class DevProfilingExtensions
{
    public static WebApplicationBuilder AddDevelopmentProfiling(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddMiniProfiler(options => options.RouteBasePath = "/profiler")
                .AddEntityFramework();
        }

        return builder;
    }

    public static WebApplication UseDevelopmentProfiling(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
            app.UseMiniProfiler();

        return app;
    }
}
#endif
