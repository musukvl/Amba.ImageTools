using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Amba.ImageTools.Infrastructure;

namespace Amba.ImageTools.Commands
{
    public class FixMediaNamesCommand : ICommand
    {
        private readonly ILogger<FixMediaNamesCommand> _logger;

        public string Name
        {
            get { return "fix-names"; }
        }

        public FixMediaNamesCommand(ILogger<FixMediaNamesCommand> logger)
        {
            _logger = logger;
        }


        private Regex _androidMediaFormatRegex = new Regex(@"(VID|IMG)_(\d\d\d\d)(\d\d)(\d\d)_(\d\d)(\d\d)(\d\d).(mp4|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public void Configure(CommandLineApplication command)
        {
            command.Description = "Gives proper name to images and videos";

            var pathArgument = command.Argument(
                "path",
                "Path to file or folder to process. Runs on current folder if empty.", false);
          
            command.HelpOption("-? | -h | --h | --help");
            command.OnExecute(() =>
            {
                var path = pathArgument.GetValue(defaultValue: Directory.GetCurrentDirectory());
                foreach (var file in Directory.GetFiles(path))
                {
                    string newName = "";

                    var match = _androidMediaFormatRegex.Match(file);                    
                    if (match.Success)
                    {
                        var year = match.Groups[2].Value;
                        var month = match.Groups[3].Value;
                        var day = match.Groups[4].Value;

                        var hour = match.Groups[5].Value;
                        var min = match.Groups[6].Value;
                        var sec = match.Groups[7].Value;
                        var extension = match.Groups[8].Value;
                        newName = $"{year}-{month}-{day} {hour}-{min}-{sec}.{extension}";
                    }
                   
                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        continue;                        
                    }

                    var folder = Path.GetDirectoryName(file);
                    var newPath = Path.Combine(folder, newName);
                    if (File.Exists(newPath))
                    {
                        newPath = Path.Combine(folder, Path.GetFileNameWithoutExtension(newName) + "_" + Path.GetFileName(file));
                    }
                    File.Move(file, newPath);
                }

                return 0;
            });
        }      
    }
}
