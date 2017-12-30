using Microsoft.Extensions.CommandLineUtils;

namespace Amba.ImageTools.Infrastructure
{
    public interface ICommand
    {
        string Name { get; }
        void Configure(CommandLineApplication command);
    }
}