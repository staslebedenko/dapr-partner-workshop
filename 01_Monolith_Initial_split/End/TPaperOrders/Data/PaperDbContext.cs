using System;
using Microsoft.EntityFrameworkCore;

namespace TPaperOrders
{
    public class PaperDbContext : DbContext
    {
        public PaperDbContext(DbContextOptions<PaperDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Client { get; set; }
        
        public DbSet<Delivery> Delivery { get; set; }
        
        public DbSet<EdiOrder> EdiOrder { get; set; }
        
        public DbSet<Inventory> Inventory { get; set; }
        
        public DbSet<Product> Product { get; set; }

        public static void ExecuteMigrations(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PaperDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var context = new PaperDbContext(optionsBuilder.Options);
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
            PaperContextConfiguration.Configure(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        /*
         * dotnet ef migrations add InitialMigration --context PaperDbContext
         * dotnet ef database update
         * dotnet ef migrations remove
        */
    }
}
