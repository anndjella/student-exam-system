using Application.Auth;
using Application.Common.Abstractions;
using Application.Services.Implementations;
using Application.Services.Interfaces;
using Application.Validators.Student;
using Domain.Common;
using Domain.Interfaces;
using FluentValidation;
using Infrastructure.Clients;
using Infrastructure.Data;
using Infrastructure.Health;
using Infrastructure.Http;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace Infrastructure;

// Everything the host needs that isn't tied to how requests come in.
// Shared by the ASP.NET Core Api and the Azure Functions host so both run the
// same services, repositories and validators.
public static class DependencyInjection
{
    public static IServiceCollection AddStudentExamCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EnableRetryOnFailure: the SQL Database is serverless and auto-pauses when idle (infra/modules/sql.bicep).
        // The first connection after a pause can fail with error 40613 ("Database is not currently
        // available") while it resumes. Retry transient connection failures during requests.
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 6,
                    maxRetryDelay: TimeSpan.FromSeconds(15),
                    errorNumbersToAdd: null)));

        // all validators live in the Application assembly
        services.AddValidatorsFromAssemblyContaining<CreateStudentValidator>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<ITeachingAssignmentRepository, TeachingAssignmentRepository>();
        services.AddScoped<ITeachingAssignmentService, TeachingAssignmentService>();
        services.AddScoped<ITermRepository, TermRepository>();
        services.AddScoped<ITermService, TermService>();
        services.AddScoped<IRegistrationRepository, RegistrationRepository>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IExamRepository, ExamRepository>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IMeService, MeService>();
        services.AddScoped<INotificationCandidateReader, NotificationCandidateReader>();

        services.AddTransient<InternalServiceResilienceHandler>();
        services.AddHttpClient<INotificationService, NotificationServiceClient>((sp, client) =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var baseUrl = cfg["Services:NotificationService"]
                ?? throw new InvalidOperationException("Missing Services:NotificationService configuration.");
            var apiKey = cfg["ServiceAuthentication:ApiKey"]
                ?? throw new InvalidOperationException("Missing ServiceAuthentication:ApiKey configuration.");

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = Timeout.InfiniteTimeSpan;
            client.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);
        })
        .AddHttpMessageHandler<InternalServiceResilienceHandler>();

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>(
                "database",
                tags: ["ready"],
                timeout: TimeSpan.FromSeconds(3))
            .AddCheck<NotificationServiceHealthCheck>(
                "notification-service",
                tags: ["ready"],
                timeout: TimeSpan.FromSeconds(3));

        services.AddScoped<TokenService>();
        services.AddScoped<AuthService>();
        services.AddSingleton<IClock, Clock>();

        return services;
    }
}
