using System;
using System.IO;
using System.Threading.Tasks;
using Amba.ImageTools.Infrastructure;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Amba.ImageTools.Commands
{
    public class ResizeCommand : ICommand
    {
        private readonly ILogger<RotateCommand> _logger;

        public string Name
        {
            get { return "resize"; }
        }

        public ResizeCommand(ILogger<RotateCommand> logger)
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

            var widthOption = command.Option("-w | --w | --width", "Angle.", CommandOptionType.SingleValue);
            var heightOption = command.Option("-h | --h | --heigth", "Angle.", CommandOptionType.SingleValue);
            var outputOption = command.Option("-o | --o | --output", "Output file or folder.",
                CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                var path = pathArgument.GetValue(defaultValue: Directory.GetCurrentDirectory());
                var pattern = patternOption.GetValue(defaultValue: "*.jpg");

                var output = outputOption.GetValue(path);

                _logger.LogInformation($@"Rotate started on path = {path}\{pattern}");

                var pathAttributes = System.IO.File.GetAttributes(path);

                var size = new Size()
                {
                    Height = heightOption.GetValue<int>(),
                    Width = widthOption.GetValue<int>(),
                };
                if (size.Height == 0 && size.Width == 0)
                {
                    Console.WriteLine("Width or height parameter not set");
                    return 1;
                }

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
                        try
                        {
                            var fileName = Path.GetFileName(file);
                            var fileOutput = Path.Combine(output, fileName);
                            ResizeImage(file, size, fileOutput);
                        }
                        catch (Exception e)
                        {
                            ConsoleLine.WriteError(file, e.Message);
                        }

                    });
                }
                else if (File.Exists(path))
                {
                    ResizeImage(path, size, output);
                }

                return 0;
            });
        }

        private static void ResizeImage(string input, Size size, string output = null)
        {
            if (string.IsNullOrWhiteSpace(output))
            {
                output = input;
            }

            using (var image = SixLabors.ImageSharp.Image.Load(input))
            {
                var originSize = image.Size();
                if ((size.Width != 0 && originSize.Width == size.Width ||
                     size.Height != 0 && originSize.Height == size.Height) && input == output)
                {
                    ConsoleLine.WriteLine(input, "Skip", ConsoleColor.DarkYellow);
                    return;
                }

                image.Mutate(x => x
                    .Resize(new ResizeOptions()
                    {
                        Mode = ResizeMode.Max,
                        Size = size
                    }));
                image.Save(output); // automatic encoder selected based on extension.
                ConsoleLine.WriteSuccess(
                    $"{input} {originSize.Width}x{originSize.Height} => {image.Width}x{image.Height} ");
            }

        }
    }
}