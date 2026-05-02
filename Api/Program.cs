
using Api.Configurations;
using Api.Services;
using Api.Settings;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Data;
using System.Security.Claims;
using System.Text;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // 1. 加入 JwtSettings 並從 appsettings.json 讀取
            var jwtSettings = builder.Configuration.GetSection(JwtSettings.Key).Get<JwtSettings>();
            builder.Services.AddSingleton(jwtSettings);
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

            // 2. 註冊 JwtService
            builder.Services.AddScoped<JwtService>();

            // 3. 配置 JWT 驗證
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
                        NameClaimType = ClaimTypes.NameIdentifier,

                        // 透過這項宣告，就可以從 "ClaimTypes.Role" 取值，並可讓 [Authorize] 判斷角色
                        RoleClaimType = ClaimTypes.Role,

                        // 一般我們都會驗證 Issuer
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

                        // 通常不太需要驗證 Audience
                        ValidateAudience = false,

                        // 一般我們都會驗證 Token 的有效期間
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,  // 避免 Token 時間漂移導致驗證失敗

                        // 簽名金鑰的驗證
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey")))
                    };
                });

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "BS.DemoShopTemplate Admin API",
                    Description = "描述..."
                });

                // 添加 JWT 安全定義
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                // 配置安全需求
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                    new string[] { }  // 空字符串表示不要求具體的權限
                }
            });
            });

            var origins = new[]
             {
                "http://localhost:5173","https://talkingtopiaadminsystem.azurewebsites.net"
            };

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(origins).AllowAnyHeader()
                                               .AllowAnyMethod()
                                               .AllowCredentials();

                });
            });

            

            builder.Services.AddInfrastructureService(builder.Configuration);
            builder.Services.AddApiService();

            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
            app.UseSwagger();
            app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
