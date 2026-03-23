using Microsoft.AspNetCore.Http;
using ProjetoFinal.Api.Services;
using ProjetoFinal.Api.Extensions;
using ProjetoFinal.Api.Factories;
using ProjetoFinal.Api.Hubs;

var app = WebApplicationBuilderFactory.CreateWebApplication(args);
app.UseCors();
app.AddSwagger();
app.UseAuthentication();
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
app.MapHub<ForumHub>("/hubs/forum");
app.MapHub<ChatHub>("/hubs/chat");
app.UseAngularFrontend();
await DataSeeder.SeedAsync(app);
app.Run();

