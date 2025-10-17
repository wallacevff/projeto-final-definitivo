using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Shared.Security;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Api.Services;

public static class DataSeeder
{
    private record SeedUser(string Username, string FullName, string Email, UserRole Role, string Password);

    private static readonly SeedUser[] DefaultUsers =
    [
        new SeedUser("wallace.vidal", "Wallace Vidal", "wallace.vidal@ead.dev", UserRole.Instructor, "123456"),
        new SeedUser("robert.leite", "Robert Leite", "robert.leite@ead.dev", UserRole.Student, "123456")
    ];

    public static async Task SeedAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync(cancellationToken);

        await EnsureDefaultUsersAsync(context, cancellationToken);

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task EnsureDefaultUsersAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        foreach (var seed in DefaultUsers)
        {
            var user = await context.Users.FirstOrDefaultAsync(
                entity => entity.Username == seed.Username,
                cancellationToken);

            if (user is null)
            {
                var now = DateTime.UtcNow;
                context.Users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    ExternalId = Guid.NewGuid(),
                    FullName = seed.FullName,
                    Email = seed.Email,
                    Username = seed.Username,
                    PasswordHash = PasswordHasher.Hash(seed.Password),
                    Role = seed.Role,
                    IsActive = true,
                    CreatedAt = now
                });
                continue;
            }

            var updated = false;

            if (!string.Equals(user.FullName, seed.FullName, StringComparison.Ordinal))
            {
                user.FullName = seed.FullName;
                updated = true;
            }

            if (!string.Equals(user.Email, seed.Email, StringComparison.OrdinalIgnoreCase))
            {
                user.Email = seed.Email;
                updated = true;
            }

            if (!string.Equals(user.Username, seed.Username, StringComparison.OrdinalIgnoreCase))
            {
                user.Username = seed.Username;
                updated = true;
            }

            if (user.Role != seed.Role)
            {
                user.Role = seed.Role;
                updated = true;
            }

            if (!PasswordHasher.Verify(seed.Password, user.PasswordHash ?? string.Empty))
            {
                user.PasswordHash = PasswordHasher.Hash(seed.Password);
                updated = true;
            }

            if (!user.IsActive)
            {
                user.IsActive = true;
                updated = true;
            }

            if (updated)
            {
                user.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
