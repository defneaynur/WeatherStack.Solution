using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using Core.Config.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Castle.Facilities.TypedFactory;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;


namespace Core.Config.Injection
{
    public static class DefaultInjection
    {
        /// <summary>
        /// Base Injections
        /// </summary>
        /// <param name="builder"></param>
        public static void BaseDefaultInjection(this WebApplicationBuilder builder)
        {
            var projectConfig = builder.GetConfigFromAppSettings<ConfigProject>();

            builder.Services.AddTransient<IConfigProject, ConfigProject>(c => projectConfig);
            builder.Services.AddTransient<IBaseInjection, BaseInjection>();

            //Injection Factory
            builder.Host
                .UseServiceProviderFactory(new WindsorServiceProviderFactory())
                .ConfigureContainer<WindsorContainer>((context, container) =>
                {
                    container.Kernel.AddFacility<TypedFactoryFacility>();
                });
        }
        private static IConfigProject _GetProjectConfig(IServiceProvider c)
        {
            var projectConfig = c.GetService<IConfigProject>();

            if (projectConfig == null)
                throw new Exception("Project config not get...");
            return projectConfig;
        }


        #region Private Methods
        public static T GetConfigFromAppSettings<T>(this WebApplicationBuilder builder)
        {
            var name = typeof(T).Name;

            var configSection = builder.Configuration.GetSection(name!);

            var config = configSection.Get<T>();
            if (config == null)
                throw new Exception($"{nameof(T)} config not get from appsettings...");
            return config;
        }


        #endregion

    }
}