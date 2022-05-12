using System.Text.Json.Serialization;
using Incremental.Common.Authentication;
using Incremental.Common.Authentication.Jwt;
using Incremental.Common.Configuration;
using Incremental.Common.Logging;
using Incremental.Common.Sourcing;
using Lamar.Diagnostics;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyAPI.Web;
using SpotiHub.Core.Application.Services.ApplicationUser;
using SpotiHub.Core.Application.Services.Spotify;
using SpotiHub.Core.Entity;
using SpotiHub.Infrastructure.Contract.Services;
using SpotiHub.Infrastructure.Contract.Services.Options;
using SpotiHub.Infrastructure.Services;
using SpotiHub.Persistence.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddCommonConfiguration();
builder.Host.UseCommonLogging();
builder.Host.UseLamar();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSourcing(AppDomain.CurrentDomain.GetAssemblies());

#region API Configuration

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddVersionedApiExplorer();
builder.Services.AddSwaggerGen();

#endregion

#region Identity

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration["CONNECTION_STRING"];
    options.UseNpgsql(connectionString, optionsBuilder =>
    {
        optionsBuilder
            .MigrationsAssembly("SpotiHub.Persistence.Migrations");
    });
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
}).AddEntityFrameworkStores<ApplicationDbContext>();

#endregion

#region Data Protection

builder.Services.AddCommonDataProtection<ApplicationDbContext>(builder.Configuration);

#endregion

#region Authentication

builder.Services.AddCors();
builder.Services.AddCommonAuthentication(builder.Configuration, options =>
{
    options.TokenValidationParameters.ValidateAudience = false;
});
builder.Services.AddJwtTokenService<ApplicationUser, ApplicationDbContext>();

#endregion

#region Integrations

builder.Services.Configure<GitHubOptions>(options =>
{
    options.ClientId = builder.Configuration["GITHUB_CLIENT_ID"];
    options.ClientSecret = builder.Configuration["GITHUB_CLIENT_SECRET"];
    options.Name = "spotihub";
});

builder.Services.Configure<SpotifyOptions>(options =>
{
    options.ClientId = builder.Configuration["SPOTIFY_CLIENT_ID"];
    options.ClientSecret = builder.Configuration["SPOTIFY_CLIENT_SECRET"];
    options.RedirectUrl = new Uri($"{builder.Configuration["JWT_TOKEN_ISSUER"]}/api/integration/spotify/authorize");
});

builder.Services.AddSingleton<SpotifyClientConfig, SpotifyClientConfig>(services => SpotifyClientConfig.CreateDefault());

builder.Services.AddScoped<ISpotifyClientFactory, SpotifyClientFactory>();
builder.Services.AddScoped<IGitHubClientFactory, GitHubClientFactory>();


#endregion

#region Application

builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();

#endregion

#region API Versioning

builder.Services.AddApiVersioning(o =>
{
    o.ReportApiVersions = true;
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.DefaultApiVersion = new ApiVersion(1, 0);
});

#endregion

#region HealthChecks

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();
builder.Services.CheckLamarConfiguration();

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction() || app.Environment.IsDevelopment())
{
    using var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope();
    serviceScope?.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
}

app.UseRouting();

app.UseCommonCors(app.Configuration);
app.UseCommonAuthentication();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
