using Api.Auth;
using Api.Middleware;
using Application.Common.Abstractions;
using Application.Auth;
using Application.Common.Errors;
using Application.DTO.Students;
using Application.DTO.Term;
using Application.Services.Interfaces;
using Application.Services.Implementations;
using Application.Validators.Enrollment;
using Application.Validators.Exam;
using Application.Validators.Student;
using Application.Validators.Subject;
using Application.Validators.Teacher;
using Application.Validators.Term;
using Domain.Common;
using Domain.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);
var allowedCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?.Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray() ?? [];

if (allowedCorsOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "Missing Cors:AllowedOrigins configuration. Configure at least one trusted frontend origin.");
}

// Services, repositories, validators, DbContext, notification client and health checks
// are shared with the Functions host (Infrastructure/DependencyInjection.cs).
builder.Services.AddStudentExamCore(builder.Configuration);

// MVC runs FluentValidation automatically before each action
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddControllers();

// Authorization requirements/handler
builder.Services.AddScoped<MustChangePasswordClearedRequirement>();
builder.Services.AddSingleton<IAuthorizationHandler, MustChangePasswordClearedHandler>();

// Middleware
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "date"
    });
    c.MapType<DateOnly?>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "date",
        Nullable = true
    });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", p =>
        p.WithOrigins(allowedCorsOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()
    );
});

builder.Services.AddJwtAuth(builder.Configuration);

// AuthZ
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PasswordChanged", p =>
        p.RequireAuthenticatedUser()
         .AddRequirements(new MustChangePasswordClearedRequirement()));
    // options.AddPolicy("PasswordChanged", p => p.RequireClaim("mcp", "true"));
});

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Schema is kept current on startup. Demo/reference data is NEVER seeded here:
// use the dedicated, explicit tools/StudentExam.DbSeeder CLI instead.
if (builder.Configuration.GetValue("Database:MigrateOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseRouting();

app.UseCors("Frontend");

app.UseHttpsRedirection();

// Exception middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
});

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}
