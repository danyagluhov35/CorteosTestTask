using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CorteosTestTask.Entity
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        public DbSet<Rate> Rates { get; set; }
        public DbSet<RateValue> RatesValues { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer("Server=host.docker.internal,1433;Database=RateDb;User Id=sa;Password=MyPasswordIsVeryGood!(@112)!!;TrustServerCertificate=True");
        }
    }
}
