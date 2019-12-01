using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Amba.ImageTools.Infrastructure;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

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


        private Regex _androidMediaFormatRegex =
            new Regex(@"(VID|IMG)_(\d\d\d\d)(\d\d)(\d\d)_(\d\d)(\d\d)(\d\d).(mp4|jpg)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private Regex _fixedFormatRegex = new Regex(@"^(\d\d\d\d)-(\d\d)-(\d\d) (\d\d)-(\d\d)-(\d\d)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private Regex _samsungFormatRegex = new Regex(@"^(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public void Configure(CommandLineApplication command)
        {
            command.Description = "Gives proper name to images and videos";

            var pathArgument = command.Argument(
                "path",
                "Path to file or folder to process. Runs on current folder if empty.", false);

            command.HelpOption("-? | -h | --h | --help");
            command.OnExecute(() =>
            {
                var imagesFolderPath = pathArgument.GetValue(defaultValue: System.IO.Directory.GetCurrentDirectory());
                ProcessFolder(imagesFolderPath);

                return 0;
            });
        }

        private void ProcessFolder(string imagesFolderPath)
        {
            foreach (var file in Directory.GetFiles(imagesFolderPath))
            {
                try
                {
                    ProcessFile(file);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void ProcessFile(string filePath)
        {
            //skip files with fixed names
            if (_fixedFormatRegex.IsMatch(Path.GetFileName(filePath)))
            {
                return;
            }

            string newName = "";
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            //try extract date from android file name
            var fixedFormatMatch = _androidMediaFormatRegex.Match(filePath);
            if (!fixedFormatMatch.Success)
            {
                fixedFormatMatch = _samsungFormatRegex.Match(filePath);
            }

            if (fixedFormatMatch.Success)
            {
                var year = fixedFormatMatch.Groups[2].Value;
                var month = fixedFormatMatch.Groups[3].Value;
                var day = fixedFormatMatch.Groups[4].Value;

                var hour = fixedFormatMatch.Groups[5].Value;
                var min = fixedFormatMatch.Groups[6].Value;
                var sec = fixedFormatMatch.Groups[7].Value;
                newName = $"{year}-{month}-{day} {hour}-{min}-{sec}{extension}";
            }

            //try extract date from EXIF
            if (string.IsNullOrWhiteSpace(newName) && extension == ".jpg")
            {
                try
                { 
                    using (var image = SixLabors.ImageSharp.Image.Load(filePath))
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
                DateTime lastWriteTime = File.GetLastWriteTime(filePath);
                if (lastWriteTime != DateTime.MinValue)
                {
                    newName = lastWriteTime.ToString("yyyy-MM-dd HH-mm-ss") + extension;
                }
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            var folder = Path.GetDirectoryName(filePath);
            var newPath = Path.Combine(folder, newName);
            if (File.Exists(newPath))
            {
                newPath = Path.Combine(folder,
                    Path.GetFileNameWithoutExtension(newName) + "_" + Path.GetFileName(filePath));
            }

            File.Move(filePath, newPath);
        }

        private DateTime? GetExifCreationDate(Image image)
        {
            if (image.Metadata?.ExifProfile?.Values == null)
                return null;
            var date = image.Metadata.ExifProfile.Values.FirstOrDefault(x => x.Tag == ExifTag.DateTimeOriginal);
            if (date == null)
            {
                date = image.Metadata.ExifProfile.Values.FirstOrDefault(x => x.Tag == ExifTag.DateTimeDigitized);
            }

            if (date == null)
            {
                date = image.Metadata.ExifProfile.Values.FirstOrDefault(x => x.Tag == ExifTag.DateTime);
            }

            if (date != null)
            {
                DateTime.TryParseExact(date.Value.ToString(), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var result);
                return result;
            }

            return null;
        }
    }
}