using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using ProjetoFinal.Api.Extensions;
using ProjetoFinal.Api.Utils;
using ProjetoFinal.Infra.CrossCutting.Providers;
using ProjetoFinal.IoC;

namespace ProjetoFinal.Api.Factories;

public static class WebApplicationBuilderFactory
{
    public static WebApplication CreateWebApplication(params string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = CustomConfigurationProvider.GetConfiguration(builder.Environment);
        builder.Configuration.AddConfiguration(configuration);
        builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
        builder.Configuration.AddEnvironmentVariables();
        builder.ConfigureControllers();
        builder.Services.ConfigureByIoC(builder.Configuration, builder.Environment);
        builder.AddSwaggerBuilder();
        builder.AddCorsBuilder();
        builder.AddJwtAuthentication();
        builder.AddRealtime();
        builder.ConfigureRequestBodySize();

        return builder.Build();
    }

    public static WebApplicationBuilder ConfigureControllers(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
            {
                options.InputFormatters.Add(
                    new PlainTextFormatter()
                );
                // options.InputFormatters.Add(new XmlSerializerInputFormatter(new MvcOptions()
                // {
                //     
                // }));
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.PropertyNamingPolicy = new PascalCaseNamingPolicy();
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });
        return builder;
    }
}
