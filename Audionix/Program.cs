using Audionix.Components.Account;
using Audionix.Components;
using Audionix.Data;
using Audionix.Models;
using Audionix.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;
using MudBlazor;
using System.Security.Cryptography.X509Certificates;
using Serilog.Settings.Configuration;
using System.Net.NetworkInformation;


var builder = WebApplication.CreateBuilder(args);

// Services
ConfigureServices(builder);

// Settings
await ConfigureSettings(builder);

// Authentication
ConfigureAuthentication(builder);

// Identity
ConfigureIdentity(builder);

// Database
ConfigureDatabase(builder);

// Logger
ConfigureLogger(builder);

// Host
ConfigureHost(builder);

var app = builder.Build();

// Database Migration
MigrateDatabase(app);

// Middleware
ConfigureMiddleware(app);

// Endpoints
ConfigureEndpoints(app);

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

    builder.Services.AddSingleton<AppStateService>();
}

async Task ConfigureSettings(WebApplicationBuilder builder)
{
    var appSettings = new AppSettings();
    var appSettingsService = new AppSettingsService(appSettings);
    builder.Services.AddSingleton(appSettingsService);

    var config = await appSettingsService.GetOrCreateConfigurationAsync();
    builder.Services.AddSingleton(config);

    Log.Information($"--- Program.cs - config.LoggingPath: {config.LoggingPath}", config.LoggingPath);
}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
}

void ConfigureIdentity(WebApplicationBuilder builder)
{
    builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
}

void ConfigureDatabase(WebApplicationBuilder builder)
{
    var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audionix", "Database", "Audionix.db");

    var databaseDirectory = Path.GetDirectoryName(databasePath);
    if (string.IsNullOrEmpty(databaseDirectory))
    {
        Log.Error("--- Program.cs - Database directory path is null or empty.");
        throw new ArgumentNullException(nameof(databaseDirectory), "Database directory path cannot be null or empty.");
    }
    Directory.CreateDirectory(databaseDirectory);
    var connectionString = $"Data Source={databasePath}";
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
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
    Log.Information("--- Program.cs - builder.Environment.EnvironmentName:  " + builder.Environment.EnvironmentName);
    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
    Log.Information("--- Program.cs - Running version: " + assembly.GetName().Version);


    if (!builder.Environment.IsDevelopment())
    {
        builder.WebHost.UseKestrel(options =>
        {
            options.ConfigureHttpsDefaults(httpsOptions =>
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
    }

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
    app.UseAntiforgery();
}

void ConfigureEndpoints(WebApplication app)
{
    app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
    app.MapAdditionalIdentityEndpoints();
    app.MapControllers();
}
