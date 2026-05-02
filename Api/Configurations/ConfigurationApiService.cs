using Api.Services;

namespace Api.Configurations
{
    public static class ConfigurationApiService
    {
        public static IServiceCollection AddApiService(this IServiceCollection services)
        {
            services.AddScoped<OrderApiService>();
            services.AddScoped<ReviewApiService>();
            services.AddScoped<BookingApiService>();
            services.AddScoped<DashboardApiService>();
            services.AddScoped<CourseManagementApiService>();
            services.AddScoped<MemberManagermentApiService>();
            services.AddScoped<CourseImageService>();
            services.AddScoped<CloudinaryService>();
            services.AddScoped<CourseManagementApiDapperService>();
            return services;
        }
    }
}
