using Audionix.Components.Account;
using Audionix.Components;
using SharedLibrary.Data;
using Audionix.Services;
using Audionix.Repositories;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;
using MudBlazor;
using System.Security.Cryptography.X509Certificates;
using Serilog.Settings.Configuration;
using DataAccess.UnitOfWork;
using Audionix.DataAccess;
using Microsoft.AspNetCore.Authentication;
using Audionix.Hubs;
using SharedLibrary.Data;
using Audionix.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Services
ConfigureServices(builder);

// Authentication
ConfigureAuthentication(builder);

// Identity
ConfigureIdentity(builder);

// Logger
ConfigureLogger(builder);

// Host
ConfigureHost(builder);

var app = builder.Build();

// Database Migration
MigrateDatabase(app);

// Seed Data
await SeedDataAsync(app);

// Middleware
ConfigureMiddleware(app);

Log.Information("--- Program.cs - Starting app.Run()");
app.Run();

void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddMudServices(config =>
    {
        config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
        config.SnackbarConfiguration.PreventDuplicates = false;
        config.SnackbarConfiguration.NewestOnTop = false;
        config.SnackbarConfiguration.ShowCloseIcon = true;
        config.SnackbarConfiguration.VisibleStateDuration = 10000;
        config.SnackbarConfiguration.HideTransitionDuration = 500;
        config.SnackbarConfiguration.ShowTransitionDuration = 500;
        config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
    })
    .AddRazorComponents()
    .AddInteractiveServerComponents();

    builder.Services.AddCascadingAuthenticationState()
    .AddScoped<FileManagerService>()
    .AddScoped<IdentityUserAccessor>()
    .AddScoped<IdentityRedirectManager>()
    .AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>()
    .AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>()
    .AddHostedService<AudionixService>()
    .AddControllers();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<IFolderRepository, FolderRepository>();
    builder.Services.AddScoped<IMusicPatternRepository, MusicPatternRepository>();
    builder.Services.AddScoped<IProgramLogRepository, ProgramLogRepository>();
    builder.Services.AddScoped<IStationRepository, StationRepository>();
    builder.Services.AddScoped<IDbContextFactory, DbContextFactory>();
    builder.Services.AddScoped<IStationRepository, StationRepository>();
    builder.Services.AddScoped<IMusicPatternRepository, MusicPatternRepository>();
    builder.Services.AddScoped<IAudioMetadataRepository, AudioMetadataRepository>();
    builder.Services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
    builder.Services.AddScoped<AppStateService>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddSignalR();

    // Add logging configuration for SignalR
    builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
    builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);

    // Add logging configuration for DevelopmentAuthenticationHandler
    builder.Logging.AddFilter("DevelopmentAuthenticationHandler", LogLevel.Information);
}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddAuthentication("Development")
            .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>("Development", options => { });
    }
    else
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();
    }
}

void ConfigureIdentity(WebApplicationBuilder builder)
{
    builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
}

void ConfigureLogger(WebApplicationBuilder builder)
{
    string _logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audionix", "Logging", "Audionix.log");
    var configuration = builder.Configuration;
    var options = new ConfigurationReaderOptions(typeof(Serilog.LoggerConfiguration).Assembly);
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration, options)
        .WriteTo.File(Path.Combine(_logPath), rollingInterval: RollingInterval.Day)
        .WriteTo.Console()
        .CreateLogger();
}

void ConfigureHost(WebApplicationBuilder builder)
{
    builder.Host.UseWindowsService();
    Log.Information("=== Audionix --- Program.cs - builder.Environment.EnvironmentName:  " + builder.Environment.EnvironmentName);
    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
    Log.Information("--- Program.cs - Running version: " + assembly.GetName().Version);

    builder.WebHost.UseKestrel(options =>
    {
        // Listen on port 443 for HTTPS traffic
        options.ListenAnyIP(443, listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
                var certificatePath = Path.Combine(AppContext.BaseDirectory, "certificate.pfx");
                if (!File.Exists(certificatePath))
                {
                    Log.Error($"Certificate file not found at path: {certificatePath}");
                    throw new FileNotFoundException($"Certificate file not found at path: {certificatePath}");
                }
                httpsOptions.ServerCertificate = new X509Certificate2(certificatePath, "Teamone1!");
            });
        });

        // Listen on port 5001 for SignalR hub
        options.ListenAnyIP(5001, listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
                var certificatePath = Path.Combine(AppContext.BaseDirectory, "certificate.pfx");
                if (!File.Exists(certificatePath))
                {
                    Log.Error($"Certificate file not found at path: {certificatePath}");
                    throw new FileNotFoundException($"Certificate file not found at path: {certificatePath}");
                }
                httpsOptions.ServerCertificate = new X509Certificate2(certificatePath, "Teamone1!");
            });
        });
    });

    if (!builder.Environment.IsDevelopment())
    {
        builder.WebHost.UseUrls("http://*:80", "https://*:443");
        builder.Services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });
    }
}

void MigrateDatabase(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            dbContext.Database.Migrate();
            Log.Information("--- Program - Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "++++++ Program.cs - An error occurred while migrating the database.");
        }
    }
}

async Task SeedDataAsync(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        try
        {
            await SeedData.Initialize(services, userManager);
            Log.Information("--- Program - Seed data completed successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "++++++ Program.cs - An error occurred while seeding the database.");
        }
    }
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseHsts();
    }
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting(); // Set up routing middleware
    app.UseAuthentication(); // Add authentication middleware
    app.UseAuthorization(); // Add authorization middleware
    app.UseAntiforgery();

    // Add the localhost restriction middleware for the registration page
    app.UseWhen(context => context.Request.Path.StartsWithSegments("/Account/Register"), appBuilder =>
    {
        appBuilder.UseMiddleware<LocalhostRestrictionMiddleware>();
    });

    // Add SignalR middleware
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapRazorComponents<App>().AddInteractiveServerRenderMode();
        endpoints.MapAdditionalIdentityEndpoints();
        endpoints.MapControllers();
        endpoints.MapHub<ProgressHub>("/progressHub");
    });
}