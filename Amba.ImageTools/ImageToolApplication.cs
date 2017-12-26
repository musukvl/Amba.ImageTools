using System;
using System.Reflection;
using Amba.ImageTools.Commands;
using Amba.ImageTools.Infrastructure;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Amba.ImageTools
{
    public class ImageToolApplication : IConsoleApplication
    {
        private readonly ILogger<ImageToolApplication> _logger;
        public IServiceProvider Services { get; set; }

        public ImageToolApplication(IServiceProvider serviceProvider, ILogger<ImageToolApplication> logger)
        {
            _logger = logger;
            Services = serviceProvider;
        }

        public static void ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<ICommand, HwCommand>();
            serviceCollection.AddSingleton<ICommand, RotateCommand>();
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
