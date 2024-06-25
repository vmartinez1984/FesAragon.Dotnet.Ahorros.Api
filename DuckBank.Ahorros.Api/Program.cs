using DuckBank.Ahorros.Api.Helpers;
using DuckBank.Ahorros.Api.HttpLoggers;
using DuckBank.Ahorros.Api.Middlewares;
using DuckBank.Ahorros.Api.Persistence;
using DuckBank.Ahorros.Api.Services;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureSerilog();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
// Add services to the container.
builder.Services.AddScoped<AhorroRepositorio>();
builder.Services.AddScoped<ClabeService>();
builder.Services.AddScoped<TarjetaDeDebitoService>();
//HttpLogger
builder.Services.AddScoped<HttpLogger>();
builder.Services.AddScoped<HttpLoggerRepository>();
//RequestResponse
builder.Services.AddTransient<RequestResponseRepository>();
//HttpClientFactory
builder.Services.AddHttpClient(string.Empty, client => { }).RemoveAllLoggers().AddLogger<HttpLogger>();
//Serilog
builder.Services.AddLogging(logger => logger.AddSerilog());

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v2.0",
        Title = "Ahorros Duckbank",
        Description = @"Cuentas de ahorro",
        Contact = new OpenApiContact
        {
            Name = "V�ctor Mart�nez",
            Url = new Uri("mailto:ahal_tocob@hotmail.com")
        }
    });
    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("*")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders("*");
    });
});

var app = builder.Build();

app.UseCors();
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(x =>
{
    x.SwaggerEndpoint("/swagger/v1/swagger.json", "");
    x.RoutePrefix = "";
});

app.UseMiddleware<RequestResponseMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
