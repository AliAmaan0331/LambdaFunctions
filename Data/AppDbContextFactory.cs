using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=test-dev-db-1.crugm6qm4vbk.ap-south-1.rds.amazonaws.com;Port=5432;Database=test-dev-db-1;Username=postgres;Password=password");
        return new AppDbContext(optionsBuilder.Options);
    }
}
