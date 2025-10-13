using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories;
using Repositories.Interfaces;
using Repositories.Implementations;
using Services.Interfaces;
using Services.Implementations;
using System.Text;
using Repositories.Models;

namespace ChargingStationSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            // ==================== DATABASE ====================
            builder.Services.AddDbContext<ChargeStationContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // ==================== DEPENDENCY INJECTION ====================
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();
//<<<<<<< HEAD

            builder.Services.AddScoped<IStationRepository, StationRepository>();
            builder.Services.AddScoped<IStationService, StationService>();

            builder.Services.AddScoped<IChargerRepository, ChargerRepository>();
            builder.Services.AddScoped<IChargerService, ChargerService>();

            builder.Services.AddScoped<IPortRepository, PortRepository>();
            builder.Services.AddScoped<IPortService, PortService>();

            builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
            builder.Services.AddScoped<IVehicleService, VehicleService>();

//=======
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IPricingRuleRepository,PricingRuleRepository>();
            builder.Services.AddScoped<IPricingRuleService,PricingRuleService>();   
//>>>>>>> Qhuy
            builder.Services.AddHttpContextAccessor(); // cần cho AuthService

            // ==================== JWT CONFIGURATION ====================
            var jwtSettings = configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"], // thêm Audience
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            // ==================== CORS ====================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });

            // ==================== CONTROLLERS + SWAGGER ====================
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ChargingStation API",
                    Version = "v1",
                    Description = "API hệ thống quản lý trạm sạc xe điện"
                });

                // Cấu hình Swagger để test JWT
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Nhập JWT token theo dạng: Bearer {token}",
                    Name = "Authorization",
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
                        new string[] {}
                    }
                });

            });

            // ==================== BUILD APP ====================
            var app = builder.Build();

            // ==================== MIDDLEWARE PIPELINE ====================
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");

            app.UseAuthentication(); // bắt buộc đặt trước Authorization
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
