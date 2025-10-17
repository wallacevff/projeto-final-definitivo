using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjetoFinal.Infra.CrossCutting.ConfigurationModels;

namespace ProjetoFinal.Api.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddSwaggerBuilder(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
            {
                var projectTitle = builder.Configuration["ProjectInfo:Title"] ?? "ProjetoFinalApi";
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = projectTitle,
                    Version = "v1"
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT no formato {seu token}"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            }
        );

        return builder;
    }

    public static WebApplicationBuilder AddCorsBuilder(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
            options.AddDefaultPolicy(op => op
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()));
        return builder;
    }

    public static WebApplicationBuilder AddJwtAuthentication(this WebApplicationBuilder builder)
    {
        var jwtConfiguration = builder.Configuration
            .GetSection(JwtConfiguration.SectionName)
            .Get<JwtConfiguration>() ?? throw new InvalidOperationException("Jwt configuration is missing.");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.Secret));

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtConfiguration.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtConfiguration.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        builder.Services.AddAuthorization();

        return builder;
    }


    public static WebApplicationBuilder ConfigureRequestBodySize(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 200 * 1024 * 1024);
        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB para uploads de arquivos
        });
        return builder;
    }
}
