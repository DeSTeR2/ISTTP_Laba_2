using Microsoft.EntityFrameworkCore;
using ProjectInfrastructure.Context;

namespace ProjectMVC.Utils.Extensions;

public static class MigrationExtension
{
    public static void ApplyMigration<TContext>(this IApplicationBuilder applicationBuilder) where TContext : DbContext
    {
        using IServiceScope serviceScope = applicationBuilder.ApplicationServices.CreateScope();
        using TContext context =
            serviceScope.ServiceProvider.GetRequiredService<TContext>();
        
        context.Database.Migrate();
    }
}