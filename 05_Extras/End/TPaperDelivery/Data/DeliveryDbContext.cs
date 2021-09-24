using System;
using Microsoft.EntityFrameworkCore;

namespace TPaperDelivery
{
    public class DeliveryDbContext : DbContext
    {
        public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Client { get; set; }
        
        public DbSet<Delivery> Delivery { get; set; }
        
        public DbSet<Inventory> Inventory { get; set; }
        
        public DbSet<Product> Product { get; set; }

        public static void ExecuteMigrations(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DeliveryDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var context = new DeliveryDbContext(optionsBuilder.Options);
            try
            {
                context.Database.Migrate();
            }
            catch (Exception e)
            {
                throw new Exception($"Error when migrating database: {e.Message}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("delivery");
            DeliveryContextConfiguration.Configure(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        /*
         * dotnet ef migrations add InitialMigration --context DeliveryDbContext
         * dotnet ef database update
         * dotnet ef migrations remove --context DeliveryDbContext
        */
    }
}
