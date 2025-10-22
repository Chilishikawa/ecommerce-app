using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerceApp.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
       public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuider = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuider.UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=ECommerceDb;Trusted_Connection=True;TrustServerCertificate=True;"
                );
            return new AppDbContext(optionsBuider.Options);
        }
    }
}
