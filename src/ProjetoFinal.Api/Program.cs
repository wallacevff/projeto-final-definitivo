using ProjetoFinal.Api.Services;
using ProjetoFinal.Api.Extensions;
using ProjetoFinal.Api.Factories;

var app = WebApplicationBuilderFactory.CreateWebApplication(args);
app.UseCors();
app.AddSwagger();
app.UseAuthorization();
app.UseMiddlewares();
app.MapControllers();
app.UseAngularFrontend();
await DataSeeder.SeedAsync(app);
app.Run();
