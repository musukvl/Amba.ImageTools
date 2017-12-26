using Microsoft.Extensions.CommandLineUtils;

namespace Amba.ImageTools.Commands
{
    public interface ICommand
    {
        string Name { get; }
        void Configure(CommandLineApplication command);
    }
}