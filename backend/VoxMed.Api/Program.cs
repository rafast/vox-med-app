using VoxMed.Auth.Extensions;
using VoxMed.Application.Interfaces.Repositories;
using VoxMed.Infrastructure.Repositories;
using VoxMed.Application.Mappings;
using VoxMed.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Program).Assembly)
    .AddControllersAsServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "VoxMed API", Version = "v1", Description = "Medical Management System API" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Add VoxMed Authentication services
builder.Services.AddVoxMedAuth(builder.Configuration);

// Add Main ApplicationDbContext (for scheduling features)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(DoctorScheduleMappingProfile));

// Add Repository services
builder.Services.AddScoped<IDoctorScheduleRepository, DoctorScheduleRepository>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:5174", "https://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VoxMed API v1");
        c.RoutePrefix = "swagger";
    });
}

// Ensure database is ready
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.EnsureAuthDatabaseAsync();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { 
    status = "Healthy", 
    service = "VoxMed API",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
})
.WithName("GetHealth")
.WithOpenApi();

// Welcome endpoint
app.MapGet("/", () => new { 
    message = "Welcome to VoxMed API", 
    version = "1.0.0",
    swagger = "/swagger",
    health = "/health"
})
.WithName("GetWelcome")
.WithOpenApi();

Console.WriteLine("🏥 VoxMed API is starting...");
Console.WriteLine($"🌐 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine("📋 Available endpoints:");
Console.WriteLine("   • Swagger UI: /swagger");
Console.WriteLine("   • Health Check: /health");
Console.WriteLine("   • Authentication: /api/auth/login");

app.Run();
