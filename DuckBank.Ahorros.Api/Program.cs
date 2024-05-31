using DuckBank.Ahorros.Api.Helpers;
using DuckBank.Ahorros.Api.HttpLoggers;
using DuckBank.Ahorros.Api.Middlewares;
using DuckBank.Ahorros.Api.Persistence;
using DuckBank.Ahorros.Api.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<AhorroRepositorio>();
builder.Services.AddScoped<ClabeService>();
builder.Services.AddScoped<TarjetaDeDebitoService>();
//HttpLogger
builder.Services.AddScoped<HttpLogger>();
builder.Services.AddScoped<HttpLoggerRepository>();
//RequestResponse
builder.Services.AddTransient<RequestResponseRepository>();
//builder.Services.AddScoped<RequestResponseMiddleware>();
//HttpClientFactory
builder.Services.AddHttpClient(string.Empty, client => { }).RemoveAllLoggers().AddLogger<HttpLogger>();
//Serilog
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseSwaggerUI();

app.UseMiddleware<RequestResponseMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
