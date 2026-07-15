using Serilog;
using SmartCampus.Configuration;
using SmartCampus.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services
    .AddOptions<BrandingSettings>()
    .Bind(builder.Configuration.GetSection(BrandingSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<SchoolSettings>()
    .Bind(builder.Configuration.GetSection(SchoolSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<IotSettings>()
    .Bind(builder.Configuration.GetSection(IotSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string DevClientCorsPolicy = "DevClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevClientCorsPolicy, policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors(DevClientCorsPolicy);
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/v1/health");

app.Run();
