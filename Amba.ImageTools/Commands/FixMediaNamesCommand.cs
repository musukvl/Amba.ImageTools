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

        private Regex _fixedFormatRegex = new Regex(@"^(\d\d\d\d)-(\d\d)-(\d\d) (\d\d)-(\d\d)-(\d\d)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                    //skip files with fixed names
                    if (_fixedFormatRegex.IsMatch(Path.GetFileName(file)))
                    {
                        continue;                        
                    }

                    string newName = "";                
                    var extension = Path.GetExtension(file).ToLowerInvariant();

                    //try extract date from android file name
                    var match = _androidMediaFormatRegex.Match(file);                    
                    if (match.Success)
                    {
                        var year = match.Groups[2].Value;
                        var month = match.Groups[3].Value;
                        var day = match.Groups[4].Value;

                        var hour = match.Groups[5].Value;
                        var min = match.Groups[6].Value;
                        var sec = match.Groups[7].Value; 
                        newName = $"{year}-{month}-{day} {hour}-{min}-{sec}.{extension}";
                    }
                    
                    //try extract date from EXIF
                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        try
                        {
                            using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load(file))
                            {
                                var creationDate = GetExifCreationDate(image);
                                if (creationDate != null)
                                {
                                    newName = creationDate.Value.ToString("yyyy-MM-dd HH-mm-ss") + extension;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            // do nothing if image reader cannot read file
                        }
                    }

                    //get datetime from file info
                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        DateTime lastWriteTime = File.GetLastWriteTime(file);
                        if (lastWriteTime != DateTime.MinValue)
                        {
                            newName = lastWriteTime.ToString("yyyy-MM-dd HH-mm-ss") + extension;
                        }
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

        private DateTime? GetExifCreationDate(Image<Rgba32> image)
        {
            if (image.MetaData?.ExifProfile?.Values == null)
                return null;
            var date = image.MetaData.ExifProfile.Values.FirstOrDefault(x => x.Tag == ExifTag.DateTimeOriginal);
            if (date == null)
            {
                date = image.MetaData.ExifProfile.Values.FirstOrDefault(x => x.Tag == ExifTag.DateTimeDigitized);
            }
            if (date == null)
            {
                date = image.MetaData.ExifProfile.Values.FirstOrDefault(x => x.Tag == ExifTag.DateTime);
            }
            if (date != null)
            {
                DateTime.TryParseExact(date.Value.ToString(), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result);
                return result;
            }

            return null;
        }
    }
}
