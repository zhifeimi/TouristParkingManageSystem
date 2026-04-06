using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using TPMS.Edge.Persistence;
using TPMS.Edge.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, configuration) =>
{
    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(new CompactJsonFormatter());
});

builder.Services.Configure<EdgeOptions>(builder.Configuration.GetSection(EdgeOptions.SectionName));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient("cloud");
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy
                .SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod();
            return;
        }

        policy
            .WithOrigins(
                "http://localhost:5080",
                "http://127.0.0.1:5080",
                "http://localhost:5173",
                "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddDbContext<EdgeDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("EdgeDatabase") ?? "Data Source=edge.db";
    options.UseSqlite(connectionString);
});

builder.Services.AddScoped<EdgeSeeder>();
builder.Services.AddScoped<EdgeLprService>();
builder.Services.AddHostedService<CloudSyncWorker>();
builder.Services.AddHostedService<MockLprWorker>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("TPMS.Edge"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics.AddRuntimeInstrumentation());

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseCors("WebClient");
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<EdgeSeeder>();
    await seeder.SeedAsync();
}

app.Run();
