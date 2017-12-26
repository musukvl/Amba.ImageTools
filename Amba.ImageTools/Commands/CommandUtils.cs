using System.ComponentModel;
using Microsoft.Extensions.CommandLineUtils;

namespace Amba.ImageTools.Commands
{
    public static class CommandUtils
    {
        public const string HelpOptionTemplate = "-? | -h | --h | --help";
        
        public static string GetValue(this CommandArgument commandArgument, string defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(commandArgument.Value))
            {
                return defaultValue;
            }
            return commandArgument.Value;
        }

        public static string GetValue(this CommandOption commandOption, string defaultValue = null)
        {
            if (!commandOption.HasValue())
            {
                return defaultValue;
            }
            return commandOption.Value();
        }

        public static T GetValue<T>(this CommandOption commandOption, T defaultValue = default(T))
        {
            if (!commandOption.HasValue())
            {
                return defaultValue;
            }
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                return (T)converter.ConvertFromString(commandOption.Value());
            }

            return defaultValue;
        }


    }
}