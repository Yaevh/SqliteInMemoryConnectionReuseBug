using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteInMemoryConnectionReuseBug.Data
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext([NotNull] DbContextOptions options) : base(options)
        {
        }


        public DbSet<Customer> Customers => Set<Customer>();
    }
}
