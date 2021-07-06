using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SqliteInMemoryConnectionReuseBug.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SqliteInMemoryConnectionReuseBug
{
    public class CustomerDbContextTests
    {
        #region building ServiceProvider
        private IServiceProvider BuildServiceProviderWithConfiguringCallback()
        {
            var services = new ServiceCollection();
            services
                .AddDbContext<CustomerDbContext>(options => options
                    .UseSqlite(BuildConnection())
                );
            return services.BuildServiceProvider();
        }

        private DbConnection BuildConnection()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection("Mode=Memory");
            connection.Open();
            return connection;
        }

        private IServiceProvider BuildServiceProviderWithExplicitConnection()
        {
            var services = new ServiceCollection();

            var connection = new Microsoft.Data.Sqlite.SqliteConnection("Mode=Memory");
            connection.Open();

            services
                .AddDbContext<CustomerDbContext>(options => options
                    .UseSqlite(connection)
                );
            return services.BuildServiceProvider();
        }

        #endregion

        [Fact]
        public async Task DbContext_with_callback_connection_fails()
        {
            var serviceProvider = BuildServiceProviderWithConfiguringCallback();

            using (var scope1 = serviceProvider.CreateScope())
            {
                var dbContext = scope1.ServiceProvider.GetRequiredService<CustomerDbContext>();
                await dbContext.Database.MigrateAsync();
            }
            
            using (var scope2 = serviceProvider.CreateScope())
            {
                var dbContext = scope2.ServiceProvider.GetRequiredService<CustomerDbContext>();
                var customers = await dbContext.Customers.ToListAsync();
                Assert.Empty(customers);
            }
        }

        [Fact]
        public async Task DbContext_with_explicit_connection_succeeds()
        {
            var serviceProvider = BuildServiceProviderWithExplicitConnection();

            using (var scope1 = serviceProvider.CreateScope())
            {
                var dbContext = scope1.ServiceProvider.GetRequiredService<CustomerDbContext>();
                await dbContext.Database.MigrateAsync();
            }

            using (var scope2 = serviceProvider.CreateScope())
            {
                var dbContext = scope2.ServiceProvider.GetRequiredService<CustomerDbContext>();
                var customers = await dbContext.Customers.ToListAsync();
                Assert.Empty(customers);
            }
        }
    }
}
