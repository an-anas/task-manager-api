using System.Diagnostics.CodeAnalysis;
using System.Text;
using MongoDB.Driver;
using Microsoft.OpenApi.Models;
using TaskManager.DataAccess.Context;
using TaskManager.DataAccess.Repository;
using TaskManager.Services;
using TaskManager.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Common.Helpers;

namespace TaskManager.API
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register ConfigurationHelper
            builder.Services.AddSingleton<IConfigurationHelper>(sp => new ConfigurationHelper(builder.Configuration));

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();

                // Add JWT authentication support to Swagger UI
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid JWT token in the text input below.\n\nExample: \"Bearer abc123\""
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

            // Register MongoDB client and context using ConfigurationHelper
            builder.Services.AddSingleton<IMongoDbContext>(sp =>
            {
                var configHelper = sp.GetRequiredService<IConfigurationHelper>();
                var mongoConnectionString = configHelper.GetConfigValue("ConnectionStrings:MongoDb");
                var databaseName = configHelper.GetConfigValue("DatabaseSettings:DatabaseName");
                var mongoClient = new MongoClient(mongoConnectionString);
                return new MongoDbContext(mongoClient, databaseName);
            });

            // Register other services
            builder.Services.AddScoped<ITaskItemRepository, TaskItemRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITaskItemService, TaskItemService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();

            // Configure JWT authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                // Get the IConfigurationHelper via the service provider
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero // Set to zero to avoid delay in token expiration
                };

                // Access the configuration helper
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Retrieve the IConfigurationHelper to get the JWT secret
                        var configHelper = context.HttpContext.RequestServices.GetRequiredService<IConfigurationHelper>();
                        var jwtSecret = configHelper.GetConfigValue("Jwt:Secret");

                        if (string.IsNullOrEmpty(jwtSecret))
                        {
                            throw new InvalidOperationException("JWT Secret is not configured.");
                        }

                        options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
                        return Task.CompletedTask;
                    }
                };
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
