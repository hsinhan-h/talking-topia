using ApplicationCore.Interfaces;
using Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Infrastructure
{
    public static class Dependencies
    {
        public static void AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("TalkingTopiaDb");
            services.AddDbContext<TalkingTopiaDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped(typeof(ITransaction), typeof(EfTransaction));
            //註冊IDbConnection for Dapper
            services.AddScoped<IDbConnection>(sp => new SqlConnection(connectionString));
        }
    }
}
