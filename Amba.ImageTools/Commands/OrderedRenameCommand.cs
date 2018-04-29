using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Amba.ImageTools.Infrastructure;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.MetaData.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;

namespace Amba.ImageTools.Commands
{
    public class OrderedRenameCommand : ICommand
    {
        private readonly ILogger<FixMediaNamesCommand> _logger;

        public string Name
        {
            get { return "ordered-names"; }
        }

        public OrderedRenameCommand(ILogger<FixMediaNamesCommand> logger)
        {
            _logger = logger;
        }

        public void Configure(CommandLineApplication command)
        {
            command.Description = "Renames images to img0000000.jpg";

            var pathArgument = command.Argument(
                "path",
                "Path to file or folder to process. Runs on current folder if empty.", false);
          
            command.HelpOption("-? | -h | --h | --help");
            command.OnExecute(() =>
            {
                var path = pathArgument.GetValue(defaultValue: Directory.GetCurrentDirectory());
                var number = 0;
                foreach (var file in Directory.GetFiles(path))
                {
                    number++;
                    var folder = Path.GetDirectoryName(file);
                    var newPath = Path.Combine(folder, "img" + number.ToString("0000000") + Path.GetExtension(file));
                    if (string.Compare(newPath, file, StringComparison.InvariantCultureIgnoreCase) == 0)
                        continue;
                    File.Move(file, newPath);
                }
                return 0;
            });
        }
 
    }
}
