using System;
using System.Collections.Async;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amba.ImageTools.Infrastructure;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace Amba.ImageTools.Commands
{
    public class RotateCommand : ICommand
    {
        private readonly ILogger<RotateCommand> _logger;

        public string Name
        {
            get { return "rotate"; }
        }

        public RotateCommand(ILogger<RotateCommand> logger)
        {
            _logger = logger;
        }

        public void Configure(CommandLineApplication command)
        {
            var pathArgument = command.Argument(
                "path",
                "Path to file or folder to process. Runs on current folder if empty.", false);

            var patternOption = command.Option(               
                "-p | --p | --pattern",
                "File matching pattern. Default is *.jpg", CommandOptionType.SingleValue);

            var angleOption = command.Option("-a | --a | --angle", "Angle.", CommandOptionType.SingleValue);
            var outputOption = command.Option("-o | --o | --output", "Output file or folder.", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                var path = pathArgument.GetValue(defaultValue: Directory.GetCurrentDirectory());
                var pattern = patternOption.GetValue(defaultValue: "*.jpg");
                var angle = angleOption.GetValue<int>(defaultValue: 90);

                var output = outputOption.GetValue(path);
    
                _logger.LogInformation($@"Rotate started on path = {path}\{pattern}");

                var pathAttributes = System.IO.File.GetAttributes(path);
                
                if (pathAttributes.HasFlag(FileAttributes.Directory))
                {
                    // check output directory exists
                    if (!Directory.Exists(output))
                    {
                        Directory.CreateDirectory(output);
                    }

                    var filesToProcess = Directory.GetFiles(path, pattern);
                    Parallel.ForEach(filesToProcess, (file) =>
                    {
                        var fileName = Path.GetFileName(file);
                        var fileOutput = Path.Combine(output, fileName);
                        RotateImage(file, angle, fileOutput);
                        new ConsoleLine().Write(file).Write(" [").Write("OK", ConsoleColor.Green).WriteLine("]");                        
                    });
                }
                else if (File.Exists(path))
                {
                    RotateImage(path, angle, output);
                }                
                return 0;
            });
        }

        private static void RotateImage(string input, int angle, string output = null)
        {
            if (string.IsNullOrWhiteSpace(output))
            {
                output = input;
            }
            using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load(input))
            {
                image.Mutate(x => x
                    .Rotate(angle));                
                image.Save(output); // automatic encoder selected based on extension.
            }
        }
    }
}
