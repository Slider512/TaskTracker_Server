using Microsoft.EntityFrameworkCore;

namespace Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
           
        }

        public DbSet<Server.Models.Task> Tasks { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // in memory database used for simplicity, change to a real db for production applications
            options.UseInMemoryDatabase("TaskTrackerDb");
        }
    }
}
