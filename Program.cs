using Hangfire;
using Microsoft.EntityFrameworkCore;
using WUIAM.Models;
using WUIAM.Services.Config.SeedService;
using Hangfire.Dashboard;
using WUIAM.Interfaces;
using WUIAM.Services;
using WUIAM.Repositories.IRepositories;
using WUIAM.Repositories;
using brevo_csharp.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using WUIAM.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// CORS configuration
string[] allowedOrigins = new[]
{
    "http://localhost:4200",
    "https://localhost:4200",
    "http://localhost:64645",
    "https://erp.uat.wigweuniversity.edu.ng",
    "https://erp.wigweuniversity.edu.ng"
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
var brevoApiKey = builder.Configuration["BrevoApi:ApiKey"];
Configuration.Default.ApiKey.Add("api-key", brevoApiKey);

var brevoApiUrl = builder.Configuration["BrevoApi:ApiUrl"];
if (!string.IsNullOrEmpty(brevoApiUrl))
{
    Configuration.Default.BasePath = brevoApiUrl;
}

// Database
builder.Services.AddDbContext<WUIAMDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Hangfire
builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddHangfireServer();


// Services & Repositories
builder.Services.AddTransient<SeedService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<INotifyService, NotifyService>();
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



builder.Services.AddHttpContextAccessor();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
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

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WUIAM API", Version = "v1" });

    // Add JWT Authentication to Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {your token}'",
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

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            new string[] {}
        }
    });
});

// Controllers
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});

// Optional console logger
builder.Services.AddSingleton<Microsoft.Extensions.Logging.ILoggerProvider, Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider>();

var app = builder.Build();

// Apply migrations

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WUIAMDbContext>();

    bool isDbCreated = db.Database.CanConnect();
    var pendingMigrations = db.Database.GetPendingMigrations();
    if (pendingMigrations.Any())
    {
        Console.WriteLine($"Applying {pendingMigrations.Count()} pending migration(s)...");

        // If the DB exists but migration history is missing, drop and recreate
        if (isDbCreated)
        {
            var appliedMigrations = db.Database.GetAppliedMigrations();
            bool hasInitialCreate = appliedMigrations.Any(m => m == "InitialCreate");
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

    // Seed Super Admin immediately after migration
    var seedService = scope.ServiceProvider.GetRequiredService<SeedService>();
    seedService.Seed();
    Console.WriteLine("Seeding completed successfully.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Migration/seed failed: {ex.Message}");
    // Do not crash the app on migration failure — log and continue
    Console.WriteLine("Continuing startup despite migration error.");
}


// Dev-only Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ? FIX: Apply CORS BEFORE authentication and authorization
app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    //Authorization = new[] { new LocalRequestsOnlyAuthorizationFilter() }
});

// Enqueue data seed job
BackgroundJob.Enqueue<SeedService>(s => s.Seed());
BackgroundJob.Enqueue<SeedService>(s => s.SeedLeaveBalancesAsync());
BackgroundJob.Enqueue<SeedService>(s => s.SeedJobCategoriesAsync());


RecurringJob.AddOrUpdate<LeaveBalanceJob>(
    "generate-leave-balances-yearly",
    job => job.GenerateLeaveBalancesForNewCycle(),
    Cron.Yearly
);

app.MapControllers();

app.Run();
