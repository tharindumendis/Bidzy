using Microsoft.EntityFrameworkCore;

namespace Bidzy.Infrastructure.Data
{
    public class DbInitializer
    {
        public static void ApplyMigrations(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            context.Database.Migrate();
        }
    }
}
