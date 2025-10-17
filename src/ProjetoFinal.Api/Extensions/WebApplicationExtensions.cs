using Microsoft.Extensions.FileProviders;
using ProjetoFinal.Api.Middlewares;

namespace ProjetoFinal.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseCors(this WebApplication app)
    {
        app.UseCors(options =>
            options.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        return app;
    }

    public static WebApplication MapFallbackToFile(this WebApplication app)
    {
        app.MapFallbackToFile("/index.html");
        return app;
    }

    public static WebApplication AddSwagger(this WebApplication app)
    {
        //if (app.Environment.IsDevelopment())
        //{
        app.UseSwagger();
        app.UseSwaggerUI();
        //}
        return app;
    }

    public static WebApplication UseStaticFiles(this WebApplication app, string rootPath)
    {
        var pathDir = Path.Combine(rootPath, "Uploads");
        if (!Directory.Exists(pathDir)) Directory.CreateDirectory(pathDir);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                pathDir),
            RequestPath = "/Uploads"
        });
        return app;
    }

    public static WebApplication UseMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMidleware>();
        return app;
    }


    public static WebApplication UseAngularFrontend(this WebApplication app)
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            ServeUnknownFileTypes = true,
        });
        app.MapFallbackToFile("index.html");
        return app;
    }
}