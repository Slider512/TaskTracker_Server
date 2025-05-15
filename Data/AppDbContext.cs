using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Models;
using ProjectTask = Server.Models.ProjectTask;

namespace Server.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Server.Models.Company> Companies { get; set; }
        public DbSet<Server.Models.Project> Projects { get; set; }
        public DbSet< Server.Models.ProjectTask> ProjectTasks { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ProjectTaskDependency> ProjectTasksDependencies { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка индексов для Task
            /*modelBuilder.Entity<ProjectTask>()
                .HasIndex(t => t.ProjectId);*/
            modelBuilder.Entity<ProjectTask>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId);

            modelBuilder.Entity<ProjectTask>()
            .HasMany(t => t.AssignedUsers)
            //.WithMany(u => u.AssignedTasks)
            //.UsingEntity(j => j.ToTable("TaskUserAssignments"))
            ;


            // Настройка Parent-Child отношения
            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.ParentTask)
                .WithMany(t => t.Subtasks)
                .HasForeignKey(t => t.ParentTaskId)
                .OnDelete(DeleteBehavior.NoAction);

            // Настройка Dependency
            /*modelBuilder.Entity<ProjectTaskDependency>()
                .HasKey(d => d.Id);*/

            modelBuilder.Entity<ProjectTaskDependency>()
                .HasOne(d => d.FromTask)
                .WithMany(t => t.OutgoingDependencies)
                .HasForeignKey(d => d.FromTaskId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<ProjectTaskDependency>()
                .HasOne(d => d.ToTask)
                .WithMany(t => t.IncomingDependencies)
                .HasForeignKey(d => d.ToTaskId)
                .OnDelete(DeleteBehavior.NoAction); 

            // Ограничение на тип связи
            modelBuilder.Entity<ProjectTaskDependency>()
                .Property(d => d.Type)
                .HasConversion<string>()
                .HasMaxLength(2)
                .IsRequired();

            // Настройка индексов
            /*modelBuilder.Entity<Server.Models.ProjectTask>()
                .HasIndex(t => t.ProjectId);*/
            modelBuilder.Entity<RefreshToken>()
                .HasKey(rt => rt.Id);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => new { a.UserId, a.Timestamp });
        }
    }
}
