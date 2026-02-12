using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WekezaSecurityProtocol.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Salama Security Protocol API",
        Version = "v1",
        Description = "Security-by-deception protocol for Wekeza Bank mobile banking",
        Contact = new OpenApiContact
        {
            Name = "Wekeza Security Team",
            Email = "security@wekeza.com"
        }
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobileApps", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure HTTP clients for Wekeza APIs
var apiSettings = builder.Configuration.GetSection("WekezaApiSettings");
builder.Services.AddHttpClient("WekezaCoreApi", client =>
{
    client.BaseAddress = new Uri(apiSettings["CoreApiBaseUrl"] ?? "https://api.wekeza.com/core");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("ComprehensiveApi", client =>
{
    client.BaseAddress = new Uri(apiSettings["ComprehensiveApiBaseUrl"] ?? "https://api.wekeza.com/comprehensive");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("Mvp4Api", client =>
{
    client.BaseAddress = new Uri(apiSettings["Mvp4ApiBaseUrl"] ?? "https://api.wekeza.com/mvp4");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register application services
// TODO: Register your services here
// builder.Services.AddScoped<ISalamaAuthenticationService, SalamaAuthenticationService>();
// builder.Services.AddScoped<IUserRepository, UserRepository>();
// builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
// builder.Services.AddScoped<IAlertService, AlertService>();
// builder.Services.AddScoped<ShadowBankingService>();
// builder.Services.AddScoped<UnifiedBankingService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Salama Security Protocol API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowMobileApps");

// Add Salama Routing Middleware (must be before authentication)
app.UseSalamaRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
});

app.Run();
