using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amba.ImageTools.Commands;
using Amba.ImageTools.Infrastructure;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Amba.ImageTools
{
    public class Application : IConsoleApplication
    {
        private readonly ILogger<Application> _logger;
        public IServiceProvider Services { get; set; }

        public Application(IServiceProvider serviceProvider, ILogger<Application> logger)
        {
            _logger = logger;
            Services = serviceProvider;
        }

        public static void ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            RegisterCommands(serviceCollection);
        }


        private static void RegisterCommands(IServiceCollection serviceCollection)
        {
            var all = Assembly
                .GetEntryAssembly()
                .GetReferencedAssemblies()
                .Union(new List<AssemblyName>{ Assembly.GetEntryAssembly().GetName()})
                .Select(Assembly.Load)
                .SelectMany(x => x.DefinedTypes)
                .Where(type => typeof(ICommand).GetTypeInfo().IsAssignableFrom(type.AsType()) && type != typeof(ICommand))
                .Select(x => x)
                .ToList();
            foreach (var ti in all)
            {
                serviceCollection.AddSingleton(typeof(ICommand), ti.AsType());                
            }
        }

        public void Run(params string[] args)
        {            
            try
            {
                var app = new CommandLineApplication(throwOnUnexpectedArg: false);
                var commands = Services.GetServices<ICommand>();
                foreach (var command in commands)
                {
                    app.Command(command.Name, command.Configure);                    
                }

                app.HelpOption("-? | -h | --h | --help");
                var versionOption = app.Option("-v | --v | --version", "Display application version", CommandOptionType.NoValue);
                app.FullName = "Amba Image Tools";
                app.Description = "Various tools for image processing.";
                app.OnExecute(() =>
                {
                    if (versionOption.HasValue())
                    {
                        Console.WriteLine(typeof(Program).Assembly.GetName().Version);
                        return 0;
                    }
                    app.ShowHelp();
                    return 0;
                });
                app.Execute(args);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Application process failed");
            }
        }
    }
}
