using System;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Amba.ImageTools.Commands
{
    public class HwCommand : ICommand
    {
        private readonly ILogger<HwCommand> _logger;

        public string Name
        {
            get { return "hw"; }
        }

        public HwCommand(ILogger<HwCommand> logger)
        {
            _logger = logger;
        }

        public void Configure(CommandLineApplication command)
        {
            command.Description = "Displays greetings message";
            var nameArgument = command.Argument("name", "Name for greeting! (world is default)");
            var greetingsCount = command.Option("-c | --c | --count", "Number of greetings (1 is default)", CommandOptionType.SingleValue);

            command.HelpOption("-? | -h | --h | --help");
            command.OnExecute(() =>
            {
                var name = nameArgument.Value ?? "world";
                var count = greetingsCount.GetValue<int>(1);

                while (count-- > 0)
                {
                    Console.WriteLine($"Hello {name}!");
                }

                //_logger.LogInformation($"Hello world information: Environment={System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
                //_logger.LogError("Hello world error");
                return 0;
            });
        }      
    }
}
