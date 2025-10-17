using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Api.Services;

public static class DataSeeder
{
    private static readonly IReadOnlyList<(string Name, string Description)> DefaultCategories =
    [
        ("Computacao", "Tecnologia, programacao e inovacao."),
        ("Matematica", "Cursos de matematica e raciocinio logico."),
        ("Lingua Portuguesa", "Comunicacao, escrita e leitura."),
        ("Gestao e Negocios", "Administracao, financas e soft skills."),
        ("Artes e Musica", "Expressao artistica, musica e criatividade.")
    ];

    public static async Task SeedAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync(cancellationToken);

        await EnsureCategoriesAsync(context, cancellationToken);
        await EnsureInstructorAsync(context, cancellationToken);

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task EnsureCategoriesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var (name, description) in DefaultCategories)
        {
            var exists = await context.CourseCategories
                .AsNoTracking()
                .AnyAsync(category => category.Name == name, cancellationToken);
            if (exists)
            {
                continue;
            }

            context.CourseCategories.Add(new CourseCategory
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                IsPublished = true
            });
        }
    }

    private static async Task EnsureInstructorAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var hasInstructor = await context.Users
            .AsNoTracking()
            .AnyAsync(user => user.Role == UserRole.Instructor, cancellationToken);
        if (hasInstructor)
        {
            return;
        }

        var instructorId = Guid.NewGuid();
        context.Users.Add(new User
        {
            Id = instructorId,
            ExternalId = instructorId,
            FullName = "Instrutor Padrao",
            Email = "instrutor@ead.dev",
            Role = UserRole.Instructor,
            Biography = "Instrutor criado automaticamente para demonstracoes.",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }
}
