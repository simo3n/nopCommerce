using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Factories;
using Nop.Plugin.Customers.AgentProfiles.Services;

namespace Nop.Plugin.Customers.AgentProfiles.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            //register services and interfaces
            services.AddScoped<IAgentModelFactory, AgentModelFactory>();
            services.AddScoped<IAgentService, AgentService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}