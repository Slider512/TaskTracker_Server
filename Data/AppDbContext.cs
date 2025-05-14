using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Models;
using Task = Server.Models.Task;

namespace Server.Data
{
    /*
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
           
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка индексов
            modelBuilder.Entity<Server.Models.Task>()
                .HasIndex(t => t.ProjectId);
            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => new { a.UserId, a.Timestamp });
        }

        public DbSet<Server.Models.Task> Tasks { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // in memory database used for simplicity, change to a real db for production applications
            //options.UseInMemoryDatabase("TaskTrackerDb");
        }
    }*/

    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet< Server.Models.Task> Tasks { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Dependency> Dependencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка индексов для Task
            modelBuilder.Entity<Task>()
                .HasIndex(t => t.ProjectId);

            // Настройка Parent-Child отношения
            modelBuilder.Entity<Task>()
                .HasOne(t => t.ParentTask)
                .WithMany(t => t.Subtasks)
                .HasForeignKey(t => t.ParentTaskId)
                .OnDelete(DeleteBehavior.SetNull);

            // Настройка Dependency
            modelBuilder.Entity<Dependency>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<Dependency>()
                .HasOne(d => d.FromTask)
                .WithMany(t => t.OutgoingDependencies)
                .HasForeignKey(d => d.FromTaskId)
                .OnDelete(DeleteBehavior.Restrict); // Запрещаем удаление, если есть исходящие зависимости

            modelBuilder.Entity<Dependency>()
                .HasOne(d => d.ToTask)
                .WithMany(t => t.IncomingDependencies)
                .HasForeignKey(d => d.ToTaskId)
                .OnDelete(DeleteBehavior.Restrict); // Запрещаем удаление, если есть входящие зависимости

            // Ограничение на тип связи
            modelBuilder.Entity<Dependency>()
                .Property(d => d.Type)
                .HasConversion<string>()
                .HasMaxLength(2)
                .IsRequired();

            // Настройка индексов
            modelBuilder.Entity<Server.Models.Task>()
                .HasIndex(t => t.ProjectId);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => new { a.UserId, a.Timestamp });
        }
    }
}
