using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ProjetoFinal.Infra.CrossCutting.Providers;

public static class CustomConfigurationProvider
{
    private const string PathToJson = "ConfigurationEnvironment";
    private const string StandardJson = "appsettings";

    public static IConfiguration GetConfiguration()
    {
        string path = Path.Combine(PathToJson, $"{StandardJson}.json");
        return new ConfigurationBuilder().AddJsonFile(path).Build();
    }

    public static IConfiguration GetConfiguration(IHostEnvironment environment)
    {
        string path;
        if (string.Equals(environment.EnvironmentName, Environments.Production))
        {
            path = Path.Combine(PathToJson, "appsettings.json");
            return new ConfigurationBuilder().AddJsonFile(path).Build();
        }

        path = Path.Combine(PathToJson, $"{StandardJson}.{Environments.Development}.json");
        return new ConfigurationBuilder().AddJsonFile(path).Build();
    }

    public static IConfiguration GetConfiguration(string environment)
    {
        string path;
        if (string.Equals(environment, Environments.Production))
        {
            path = Path.Combine(PathToJson, "appsettings.json");
            return new ConfigurationBuilder().AddJsonFile(path).Build();
        }

        path = Path.Combine(PathToJson, $"{StandardJson}.{Environments.Development}.json");
        return new ConfigurationBuilder().AddJsonFile(path).Build();
    }
}