using Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudentExam.Functions.Common;

var builder = FunctionsApplication.CreateBuilder(args);

// ASP.NET Core integration: functions get HttpRequest/IActionResult, same as the old controllers
builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// same services, repositories, validators and health checks as the Api
builder.Services.AddStudentExamCore(builder.Configuration);

// auth + validation + error mapping every HTTP function runs through
builder.Services.AddSingleton<JwtAuthenticator>();
builder.Services.AddScoped<FunctionEndpoint>();

builder.Build().Run();
