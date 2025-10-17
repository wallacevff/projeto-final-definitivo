using ProjetoFinal.Api.Extensions;
using ProjetoFinal.Api.Factories;

var app = WebApplicationBuilderFactory.CreateWebApplication(args);
app.UseCors();
app.AddSwagger();
app.UseAuthorization();
app.UseMiddlewares();
app.MapControllers();
app.UseAngularFrontend();
app.Run();