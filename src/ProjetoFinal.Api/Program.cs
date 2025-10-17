using Microsoft.AspNetCore.Http;
using ProjetoFinal.Api.Services;
using ProjetoFinal.Api.Extensions;
using ProjetoFinal.Api.Factories;

var app = WebApplicationBuilderFactory.CreateWebApplication(args);
app.UseCors();
app.AddSwagger();
app.UseAuthorization();
app.UseMiddlewares();
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/v1", out var remainder))
    {
        context.Request.Path = new PathString("/api") + remainder;
    }

    await next();
});
app.MapControllers();
app.UseAngularFrontend();
await DataSeeder.SeedAsync(app);
app.Run();

