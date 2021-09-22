using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TPaperOrders
{
    public class PaperContextFactory : IDesignTimeDbContextFactory<PaperDbContext>
    {
        public PaperDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PaperDbContext>();

            string sqlString = Environment.GetEnvironmentVariable("SqlPaperString");
            string sqlPassword = Environment.GetEnvironmentVariable("SqlPaperPassword");
            string connectionString = new SqlConnectionStringBuilder(sqlString) { Password = sqlPassword }.ConnectionString;
            
            optionsBuilder.UseSqlServer(connectionString);

            return new PaperDbContext(optionsBuilder.Options);
        }
    }
}