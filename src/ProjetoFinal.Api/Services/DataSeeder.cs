using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Shared.Security;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Api.Services;

public static class DataSeeder
{
    private record SeedUser(Guid Id, Guid ExternalId, string Username, string FullName, string Email, UserRole Role, string Password);

    private static readonly SeedUser[] DefaultUsers =
    [
        new SeedUser(
            Id: Guid.Parse("7f2b0cd9-6e3c-4f58-9f3e-08dba86fbde1"),
            ExternalId: Guid.Parse("068df3c8-9b36-4e44-9ba1-3c5a2e41a9bd"),
            Username: "wallace.vidal",
            FullName: "Wallace Vidal",
            Email: "wallace.vidal@ead.dev",
            Role: UserRole.Instructor,
            Password: "123456"),
        new SeedUser(
            Id: Guid.Parse("f407e6da-4b6a-4c81-92d3-000fcdab3047"),
            ExternalId: Guid.Parse("b18431db-e4fb-4a1d-a2a9-d9e6f85ba521"),
            Username: "robert.leite",
            FullName: "Robert Leite",
            Email: "robert.leite@ead.dev",
            Role: UserRole.Student,
            Password: "123456")
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
                    Id = seed.Id,
                    ExternalId = seed.ExternalId == Guid.Empty ? Guid.NewGuid() : seed.ExternalId,
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

            if (seed.ExternalId != Guid.Empty && user.ExternalId != seed.ExternalId)
            {
                user.ExternalId = seed.ExternalId;
                updated = true;
            }

            if (updated)
            {
                user.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
