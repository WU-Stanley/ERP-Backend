using Hangfire;
using Microsoft.EntityFrameworkCore;
using WUIAM.Models;
using WUIAM.Services.Config.SeedService;
using Hangfire.Dashboard;
using WUIAM.Interfaces;
using WUIAM.Services;
using WUIAM.Repositories.IRepositories;
using WUIAM.Repositories;
using WUIAM.Middleware;
using WUIAM.Hubs;
using brevo_csharp.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using WUIAM.Jobs;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
// Force restart for DI registration
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Serilog structured logging with rolling files
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithCorrelationId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.RollingFile(
        pathFormat: $"Logs/log-{DateTime.Now:yyyy-MM-dd}-{Guid.NewGuid():N}.txt",
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10_000_000)
    .CreateLogger());

// Add services to the container.

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ConfigureEndpointDefaults(listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http1;
        });
    });
}

// CORS configuration
string[] allowedOrigins = new[]
{
    "http://localhost:4200",
    "https://localhost:4200",
    "http://localhost:64645",
    "https://erp.uat.wigweuniversity.edu.ng",
    "https://erp.wigweuniversity.edu.ng",
    "https://wigweuniversity.edu.ng",
    "https://www.wigweuniversity.edu.ng"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy => policy.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
});

// Configure Brevo (SendinBlue) API
var brevoApiKey = builder.Configuration["Brevo:ApiKey"];
Configuration.Default.ApiKey.Add("api-key", brevoApiKey);

var brevoApiUrl = builder.Configuration["Brevo:ApiUrl"];
if (!string.IsNullOrEmpty(brevoApiUrl))
{
    var brevoUri = new Uri(brevoApiUrl);
    var brevoBasePath = brevoUri.GetLeftPart(UriPartial.Path).TrimEnd('/');

    if (brevoBasePath.EndsWith("/smtp/email", StringComparison.OrdinalIgnoreCase))
    {
        brevoBasePath = brevoBasePath[..^"/smtp/email".Length];
    }

    Configuration.Default.BasePath = brevoBasePath;
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
bool hasDatabase = !string.IsNullOrEmpty(connectionString);

// Database
if (hasDatabase)
{
    builder.Services.AddDbContext<WUIAMDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// Hangfire - only if database is available
bool hangfireEnabled = false;
if (hasDatabase)
{
    builder.Services.AddHangfire(config =>
    {
        config.UseSqlServerStorage(connectionString);
    });
    builder.Services.AddHangfireServer();
    hangfireEnabled = true;
}


// Memory cache for caching
builder.Services.AddMemoryCache();

// Health checks
builder.Services.AddHealthChecks();

// Response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[]
    {
        "application/json",
        "application/xml",
        "text/plain",
        "text/html",
        "text/css",
        "application/javascript"
    };
});

// Services & Repositories
builder.Services.AddTransient<SeedService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddHttpClient<INotifyService, NotifyService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<ILeaveRequestApprovalRepository, LeaveRequestApprovalRepository>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<IApprovalFlowRepository, ApprovalFlowRepository>();
builder.Services.AddScoped<IApprovalStepRepository, ApprovalStepRepository>();
builder.Services.AddScoped<IApprovalFlowService, ApprovalFlowService>();
builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();
builder.Services.AddScoped<IPublicHolidayRepository, PublicHolidayRepository>();
builder.Services.AddScoped<IPublicHolidayService, PublicHolidayService>();
builder.Services.AddScoped<ILeavePolicyRepository, LeavePolicyRepository>();
builder.Services.AddScoped<ILeaveDateCalculator, LeaveDateCalculator>();
builder.Services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
builder.Services.AddScoped<ILeaveApprovalService, LeaveApprovalService>();
        builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        builder.Services.AddScoped<IEmployeeService, EmployeeService>();
        builder.Services.AddScoped<IEmploymentRepository, EmploymentRepository>();
        builder.Services.AddScoped<IEmploymentService, EmploymentService>();
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        builder.Services.AddScoped<IAuditLogService, AuditLogService>();
        builder.Services.AddScoped<ICachingService, CachingService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();

        // Recruitment Management Services
        builder.Services.AddScoped<IRecruitmentService, RecruitmentService>();
        builder.Services.AddScoped<IAiResumeScanningService, AiResumeScanningService>();
        builder.Services.AddScoped<ITeamsMeetingService, TeamsMeetingService>();
        builder.Services.AddHttpClient<IMicrosoftAccountProvisioningService, MicrosoftAccountProvisioningService>();
        builder.Services.AddHttpClient<IAiResumeScanningService, AiResumeScanningService>();
        builder.Services.AddHttpClient<ITeamsMeetingService, TeamsMeetingService>();
        
        // Registry Integration
        builder.Services.AddScoped<IRegistrySyncService, RegistrySyncService>();
        builder.Services.AddHttpClient("RegistrySyncClient");

        // Attendance Management
        builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        builder.Services.AddScoped<IAttendanceService, AttendanceService>();
        builder.Services.AddScoped<IExportService, ExportService>();

builder.Services.AddHttpContextAccessor();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new { message = "Rate limit exceeded. Please try again later." });
    };
});

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.ASCII.GetBytes(
                builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured.")
            )
        ),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Global authorization policy - protect all routes by default
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-API-Version"));
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger with XML documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WUIAM API",
        Version = "v1",
        Description = "Wigwe University Identity & Access Management REST API\n\n" +
            "Comprehensive API for managing users, roles, permissions, departments, " +
            "employees, and leave workflows in Wigwe University.",
        Contact = new OpenApiContact
        {
            Name = "WUIAM Support",
            Email = "support@wigweuniversity.edu.ng",
            Url = new Uri("https://erp.wigweuniversity.edu.ng")
        },
        License = new OpenApiLicense
        {
            Name = "Internal Use Only",
            Url = new Uri("https://wigweuniversity.edu.ng")
        }
    });

    // Include XML documentation comments
    var xmlFile = $"{typeof(WUIAM.Controllers.AuthController).Assembly.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        o.IncludeXmlComments(xmlPath);

    // Group controllers by namespace for better organization
    o.DocInclusionPredicate((docName, apiDesc) =>
    {
        var apiVersion = apiDesc.ActionDescriptor?.RouteValues?.ContainsKey("controller") == true
            ? apiDesc.ActionDescriptor.RouteValues["controller"]?.Split('/').FirstOrDefault()
            : null;
        if (string.IsNullOrEmpty(apiVersion)) return true;
        return apiVersion.Equals(docName, StringComparison.OrdinalIgnoreCase);
    });

    // Use operation IDs for better tracing
    o.CustomOperationIds(apiDesc =>
    {
        var controller = apiDesc.ActionDescriptor?.RouteValues?["controller"] ?? "Unknown";
        var httpMethod = apiDesc.HttpMethod ?? "GET";
        return $"{controller}_{httpMethod}";
    });

    // Add security definition
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter your JWT Bearer token in the format: `Bearer {token}`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    o.AddSecurityDefinition("Bearer", securityScheme);

    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            new string[] {}
        }
    });

    // Enable DTO schema descriptions from XML comments
    o.CustomSchemaIds(type =>
    {
        var id = type.FullName;
        if (id == null) return type.Name!;
        // Remove generic markers: `1, [, ], etc.
        var cleaned = id.Replace('`', '\\').Replace('<', '\\').Replace('>', '\\');
        return cleaned.Replace('\\', ' ');
    });
});

// Controllers
builder.Services.AddControllers(options =>
{
    options.Filters.Add<WUIAM.Middleware.AuditLogActionFilter>();
}).AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
});

builder.Services.AddSignalR();

var app = builder.Build();

// Apply migrations
try
{
    if (hasDatabase)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WUIAMDbContext>();

        bool isDbCreated = db.Database.CanConnect();
        var pendingMigrations = db.Database.GetPendingMigrations();
        if (pendingMigrations.Any())
        {
            Console.WriteLine($"Applying {pendingMigrations.Count()} pending migration(s)...");

            if (isDbCreated)
            {
                var appliedMigrations = db.Database.GetAppliedMigrations();
                bool hasInitialCreate = appliedMigrations.Any(m => m.EndsWith("InitialCreate"));
                if (!hasInitialCreate)
                {
                    Console.WriteLine("WARNING: Database exists but migration history is missing. " +
                        "Dropping database to allow a clean migration run...");
                    db.Database.EnsureDeleted();
                    Console.WriteLine("Database dropped. Recreating via migrations...");
                    db.Database.Migrate();
                }
                else
                {
                    db.Database.Migrate();
                }
            }
            else
            {
                db.Database.Migrate();
            }
            Console.WriteLine("Migration completed successfully.");
        }
        else
        {
            Console.WriteLine("No pending migrations. Database is up to date.");
        }

        var seedService = scope.ServiceProvider.GetRequiredService<SeedService>();
        seedService.Seed();
        Console.WriteLine("Seeding completed successfully.");

        await WUIAM.SeedData.DatabaseSeeder.SeedAsync(db);
        Console.WriteLine("Extended seeding completed successfully.");
    }
    else
    {
        Console.WriteLine("WARNING: No database connection string configured. Skipping migrations and seeding.");
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Migration/seed failed: {ex.Message}");
    Console.WriteLine("Continuing startup despite migration error.");
}


// Dev-only Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handling should be early enough to catch middleware and auth failures.
app.UseMiddleware<WUIAM.Middleware.GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

// ? FIX: Apply CORS BEFORE authentication and authorization
app.UseCors("AllowAllOrigins");

// Correlation ID middleware (must be early in pipeline)
app.UseCorrelationId();

// Performance metrics middleware
app.UsePerformanceMetrics();

// Response compression (Gzip/Brotli)
app.UseResponseCompression();
app.UseStaticFiles();

// Serve files from uploads folder (recruitment resumes, etc.)
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// Rate limiting
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Health checks endpoint
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            environment = app.Environment.EnvironmentName,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds.ToString("F2") + "ms"
            }).ToList(),
            metrics = WUIAM.Middleware.PerformanceMetricsMiddleware.GetMetrics().Select(m => new
            {
                endpoint = m.Key,
                count = m.Value.Count,
                avgMs = m.Value.AverageMilliseconds.ToString("F2"),
                minMs = m.Value.MinMilliseconds.ToString("F2"),
                maxMs = m.Value.MaxMilliseconds.ToString("F2")
            }).ToList()
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

// Health check controller for custom checks
app.MapGet("/health/details", ([FromServices] WUIAMDbContext db) =>
{
    return new
    {
        database = db.Database.CanConnect() ? "healthy" : "unhealthy",
        timestamp = DateTime.UtcNow
    };
});

if (hangfireEnabled)
{
    // Hangfire Dashboard
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        //Authorization = new[] { new LocalRequestsOnlyAuthorizationFilter() }
    });

    try
    {
        BackgroundJob.Enqueue<SeedService>(s => s.SeedLeaveBalancesAsync());
        BackgroundJob.Enqueue<SeedService>(s => s.SeedJobCategoriesAsync());

        RecurringJob.AddOrUpdate<LeaveBalanceJob>(
            "generate-leave-balances-yearly",
            job => job.GenerateLeaveBalancesForNewCycle(),
            Cron.Yearly
        );

        RecurringJob.AddOrUpdate<WUIAM.Jobs.RegistrySyncJob>(
            "registry-integration-sync-hourly",
            job => job.ExecuteAsync(),
            Cron.Hourly
        );
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to initialize Hangfire background jobs: {ex.Message}");
    }
}

app.MapControllers();
app.MapHub<NotificationHub>("/api/notificationsHub").RequireAuthorization();

app.Run();
