using Amba.ImageTools.Infrastructure;

namespace Amba.ImageTools
{
    public class Program
    {
        static void Main(string[] args)
        {
            var applicationBuilder = new ApplicationBuilder<ImageToolApplication>();
            var application = applicationBuilder
                .ReadConfiguration()
                .RegisterServices((serviceCollection, configuration) => ImageToolApplication.ConfigureServices(serviceCollection, configuration))
                .Build();
            application.Run(args);
        }        
    }
}