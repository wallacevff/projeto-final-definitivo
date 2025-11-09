using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;
using ProjetoFinal.Infra.CrossCutting.ConfigurationModels;

namespace ProjetoFinal.Infra.CrossCutting.Storage;

public static class ObjectStorageServiceCollectionExtensions
{
    public static IServiceCollection AddObjectStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<MinioConfiguration>()
            .Bind(configuration.GetSection(MinioConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IMinioClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<MinioConfiguration>>();
            return BuildClient(options.Value);
        });

        services.AddSingleton<IObjectStorageService, MinioObjectStorageService>();
        return services;
    }

    private static IMinioClient BuildClient(MinioConfiguration configuration)
    {
        if (!Uri.TryCreate(configuration.Endpoint, UriKind.Absolute, out var endpoint))
        {
            throw new InvalidOperationException("Minio endpoint configuration is invalid.");
        }

        var builder = new MinioClient()
            .WithEndpoint(endpoint.Host, endpoint.Port)
            .WithCredentials(configuration.AccessKey, configuration.SecretKey);

        if (string.Equals(endpoint.Scheme, "https", StringComparison.OrdinalIgnoreCase))
        {
            builder = builder.WithSSL();
        }

        if (!string.IsNullOrWhiteSpace(configuration.Region))
        {
            builder = builder.WithRegion(configuration.Region);
        }

        return builder.Build();
    }
}
