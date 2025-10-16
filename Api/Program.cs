using Microsoft.EntityFrameworkCore;
using Infrastructure;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Application.DTO.Students;
using FluentValidation;
using Application.Common;
using FluentValidation.AspNetCore;
using Api.Filter;
using Application.ServicesImplementation;
using Application.Validators.Student;
using Application.Validators.Teacher;
using Application.Validators.Subject;
using Application.Validators.Exam;
using Api.Middleware;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateStudentValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateStudentValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTeacherValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateTeacherValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateSubjectValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateSubjectValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateExamValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateExamValidator>();

builder.Services.AddControllers();


builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IExamService, ExamService>();
//builder.Services.AddControllers(o =>
//{
//    o.Filters.Add<ApiExceptionFilter>();
//});
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

builder.Services.AddProblemDetails();

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
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontDev", p =>
        p.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
         .AllowAnyHeader()
         .AllowAnyMethod()
    );
});
var app = builder.Build();
app.UseRouting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
app.UseCors("FrontDev");
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
