using Web.Data;
using Web.Repository;
using Web.Configurations;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using ApplicationCore.Interfaces;
using ApplicationCore.Services;
using Infrastructure.Service;
using Web.Hubs;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Web.Models.MongoDB;
using Web.Settings;
using Web.Helpers;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace Web
{
    public class Program
    {
        [Experimental("SKEXP0020")]
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>();
            }

            builder.Services.AddMemoryCache();
            //註冊 CacheService
            builder.Services.AddScoped<CacheService>();

            // Add DbContext configuration
            builder.Services.AddDbContext<Data.TalkingTopiaDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("TalkingTopiaDb")));



            //註冊IRepository
            builder.Services.AddScoped<IRepository, GeneralRepository>();

            //註冊LineAuthService
            builder.Services.AddScoped<ILineAuthService, LineAuthService>();

            //註冊EmailService
            builder.Services.AddScoped<IEmailService, EmailService>();

            //註冊SearchService
            builder.Services.AddScoped<ISearchService, ApplicationCore.Services.SearchService>();


            builder.Services.AddScoped<DifySearchRecommendationService>();

            builder.Services.AddScoped<Web.Services.OpenAI.OpenAIService>();


            //註冊Coravel Scheduler
            builder.Services.AddScheduler();

            builder.Services.AddHttpContextAccessor();

            //註冊redis distributed cache
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration["ConnectionStrings:Redis"];
                options.InstanceName = "TalkingTopia-Cache";
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // 啟用 Session 支援
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // 設定 Session 過期時間
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddHttpClient();

            //builder.Services.AddScoped<IHostedService,BackgroundTaskService>();
            builder.Services.AddScoped<Services.BookingService>();
            builder.Services.AddScoped<CourseService>();
            builder.Services.AddScoped<MemberDataService>();
            builder.Services.AddScoped<ResumeDataService>();
            builder.Services.AddScoped<TutorDataservice>();
            builder.Services.AddScoped<AccountService>();
            builder.Services.AddScoped<NationService>();
            builder.Services.AddScoped<CourseCategoryService>();
            builder.Services.AddScoped<CloudinaryService>();
            builder.Services.AddScoped<RedisCacheHelper>();
            builder.Services.AddScoped<AppointmentNotificationService>();

            // 要加下面這個 AddInfrastructureService      
            builder.Services.AddInfrastructureService(builder.Configuration);
            // 將DI改至Configurations資料夾內的兩支檔案，若有改就可以把上方那一排Service注入個別刪除
            // ConfigureApplicationCoreService -> for 非Web專案內的DI
            // ConfigureWebService -> for Web專案內的DI
            builder.Services.AddApplicationCoreService().AddWebService();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("https://localhost:7263")
                               .AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .WithExposedHeaders("*");

                        //builder.WithOrigins("http://example.com","http://www.contoso.com")
                        //       .WithMethods("GET", "POST", "PUT", "DELETE");
                    });
            });

            // Add SignalR
            builder.Services.AddSignalR();

            // Add MongoDB settings
            builder.Services
            .Configure<MongoDbSettings>(builder.Configuration.GetSection(nameof(MongoDbSettings)))
            .AddSingleton(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<MongoDbSettings>();
                return new MongoClient(settings.ConnectionString);
            });
            builder.Services.AddScoped<MongoRepository>();

            // Product Semantic Search Service
            builder.Services
                .Configure<ApplicationCore.Settings.MongoDbVecotrSearchSettings>(builder.Configuration.GetSection(nameof(ApplicationCore.Settings.MongoDbVecotrSearchSettings)));



            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                            .AddCookie(options =>
                            {
                                // 登入用路徑
                                options.LoginPath = "/Account/Login";
                                // 登出用路徑
                                options.LogoutPath = "/Account/Logout";
                                // 沒有權限時的導向(HTTP Status Code: 403)
                                options.AccessDeniedPath = "/Account/AccessDenied";
                                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);  // 預設過期時間
                            });

            builder.Services.AddAuthorization();
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            



            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }



            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // 啟用 Session
            app.UseSession();

            app.UseCors();

            // 先驗證再授權.
            app.UseAuthentication();
            app.UseAuthorization();

            //設置Coravel排程
            var scheduler = app.Services.GetRequiredService<IScheduler>();
            scheduler.Schedule(async () =>
            {
                using var scope = app.Services.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<AppointmentNotificationService>();
                await notificationService.SendNotificationsAsync();
            }).EveryMinute();

            app.MapControllers();


            app.MapControllerRoute(
            name: "Login",
            pattern: "Login",
            defaults: new { controller = "Account", action = "Account" });

            app.MapControllerRoute(
            name: "MemberData",
            pattern: "MemberData",
            defaults: new { controller = "Member", action = "MemberData" });

            app.MapControllerRoute(
            name: "MemberTransaction",
            pattern: "MemberOrderDetails",
            defaults: new { controller = "Member", action = "MemberTransaction" });

            app.MapControllerRoute(
            name: "TutorData",
            pattern: "TutorData",
            defaults: new { controller = "Tutor", action = "TutorData" });

            app.MapControllerRoute(
            name: "PublishCourse",
            pattern: "PublishCourse",
            defaults: new { controller = "Tutor", action = "PublishCourse" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<ChatHub>("/chatHub");

            app.Run();
        }


    }
}
