using ApplicationCore.Interfaces;
using ApplicationCore.Services;
using Infrastructure.ECpay;
using Infrastructure.Interfaces.ECpay;
using Infrastructure.Service;
using System.Diagnostics.CodeAnalysis;

namespace Web.Configurations
{
    [Experimental("SKEXP0020")]
    public static class ConfigureApplicationCoreService
    {
        public static IServiceCollection AddApplicationCoreService(this IServiceCollection services)
        {
            services.AddScoped<IPayment, Payment>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IShoppingCartService, ShoppingCartService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ICourseService, AppCourseService>();
            services.AddScoped<ECpayService>();
            services.AddScoped<ICourseService, AppCourseService>();
            services.AddScoped<IOpenAIService, OpenAIService>();
            services.AddScoped<IVectorSearchService, VectorSearchServices>();


            return services;
        }
    }
}
