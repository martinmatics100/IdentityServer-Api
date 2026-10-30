using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SchoolPilot_IdentityServer.Common.Enums;
using SchoolPilot_IdentityServer.Data.Context;
using SchoolPilot_IdentityServer.Data.Contracts;
using SchoolPilot_IdentityServer.Data.Entities;
using SchoolPilot_IdentityServer.Data.Helpers;
using System.Text;

namespace SchoolPilot_IdentityServer.Extensions
{
    public static partial class ApplicationService
    {
        public static void ConfigureCors(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    if (env.IsDevelopment())
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    }
                    else
                    {
                        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();
                        builder.WithOrigins(allowedOrigins!)
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    }
                });
            });
        }


        public static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentityCore<ApplicationUser>(o =>
            {
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = true;
                o.Password.RequireUppercase = false;
                o.Password.RequiredLength = 8;
            }).AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        }

        public static void ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            Console.WriteLine($"JWT Settings: {System.Text.Json.JsonSerializer.Serialize(jwtSettings)}");
            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key))

            {
                throw new InvalidOperationException("JWT secret key is not configured.");
            }

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.ValidIssuer,
                    ValidAudience = jwtSettings.ValidAudience,
                    IssuerSigningKey = secretKey
                };
                o.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            message = "You are not authorized to access this resource. Please authenticate."
                        });
                        return context.Response.WriteAsync(result);
                    },
                };
            });

        }

        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            string[] roles = { UserRole.USER.ToString(), UserRole.ADMIN.ToString(), UserRole.PROVIDERS.ToString() };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var identityRole = new IdentityRole<Guid>
                    {
                        Id = SequentialGuid.Create(),
                        Name = role,
                        NormalizedName = role.ToUpper()
                    };

                    await roleManager.CreateAsync(identityRole);
                }
            }
        }

    }
}
