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
using Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

// Db
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateStudentValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateStudentValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateEnrollmentsValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTermValidator>();

builder.Services.AddControllers();

// DI
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ITeachingAssignmentRepository, TeachingAssignmentRepository>();
builder.Services.AddScoped<ITeachingAssignmentService, TeachingAssignmentService>();
builder.Services.AddScoped<ITermRepository, TermRepository>();
builder.Services.AddScoped<ITermService, TermService>();
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IMeService, MeService>();

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();

// Authorization requirements/handler
builder.Services.AddScoped<MustChangePasswordClearedRequirement>();
builder.Services.AddSingleton<IAuthorizationHandler, MustChangePasswordClearedHandler>();
builder.Services.AddSingleton<IClock, Clock>();

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
    {
        var peopleSeeder = new PeopleUsersSeeder(db);
        await peopleSeeder.SeedAsync();

        var enrollmentSeeder = new EnrollmentSeeder(db);
        await enrollmentSeeder.SeedAsync();

        var registrationsExamsSeeder = new RegistrationsExamsSeeder(db);
        await registrationsExamsSeeder.SeedAsync(2002);

    }
    else
    {
        var seedMode = builder.Configuration["SeedData:Mode"] ?? "None";
        if (seedMode.Equals("Demo", StringComparison.OrdinalIgnoreCase))
        {
            var timeZoneId = builder.Configuration["SeedData:TimeZone"] ?? "Europe/Budapest";
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);

            var demoSeeder = new ProductionDemoSeeder(db);
            var result = await demoSeeder.SeedAsync(
                DateOnly.FromDateTime(localNow.DateTime));

            app.Logger.LogInformation(
                "Production demo seed {Action}. Student services: {StudentServices}, students: {Students}, " +
                "teachers: {Teachers}, subjects: {Subjects}, terms: {Terms}, assignments: {Assignments}, " +
                "enrollments: {Enrollments}, registrations: {Registrations}, exams: {Exams}.",
                result.WasCreated ? "created" : "already present",
                result.StudentServices,
                result.Students,
                result.Teachers,
                result.Subjects,
                result.Terms,
                result.TeachingAssignments,
                result.Enrollments,
                result.Registrations,
                result.Exams);
        }
        else if (!seedMode.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Unsupported SeedData:Mode '{seedMode}'. Allowed values are None and Demo.");
        }
    }
}

app.UseRouting();

app.UseCors("Frontend");

app.UseHttpsRedirection();

// Exception middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}
