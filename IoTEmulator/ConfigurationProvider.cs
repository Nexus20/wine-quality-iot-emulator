using Microsoft.Extensions.Configuration;

namespace IoTEmulator;

public static class ConfigurationProvider
{
    public static IConfiguration Configuration { get; private set; }

    public static void SetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        Configuration = builder.Build();
    }
}