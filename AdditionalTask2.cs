using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO.NET_HW_16_10
{
    public class AdditionalTask2
    {
        static void Main(string[] args)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var projects = db.Projects.Include(p => p.Tasks).ThenInclude(t => t.Employees).ToList();
                if (projects[0] != null)
                {
                    db.Projects.Remove(projects[0]);
                    db.SaveChanges();
                }

                var projectWhereWorks = db.Projects.Where(p=>p.Tasks.Any(t => t.Employees
                    .Any(e => e.FullName.Equals("John Doe")))).ToList();
            }
        }
    }

    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Task> Tasks { get; set; }
    }

    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public Status Status { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public List<Employee> Employees { get; set; }
    }

    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public Position Position { get; set; }
        public List<Task> Tasks { get; set; }
    }

    public enum Status
    {
        Completed,
        InProgress,
        Cancelled
    }

    public enum Position
    {
        Junior,
        Middle,
        Senior
    }

    public class ApplicationContext : DbContext
    {
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Task> Tasks { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Tasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId);

            modelBuilder.Entity<Project>().HasData(
                new Project { Id = 1, Title = "Project Alpha", Description = "This is the Alpha project" },
                new Project { Id = 2, Title = "Project Beta", Description = "This is the Beta project" }
            );

            modelBuilder.Entity<Task>().HasData(
                new Task { Id = 1, Title = "Task A1", Description = "Alpha Task 1", Deadline = DateTime.Now.AddDays(10), Status = Status.InProgress, ProjectId = 1 },
                new Task { Id = 2, Title = "Task A2", Description = "Alpha Task 2", Deadline = DateTime.Now.AddDays(20), Status = Status.Completed, ProjectId = 1 },
                new Task { Id = 3, Title = "Task B1", Description = "Beta Task 1", Deadline = DateTime.Now.AddDays(5), Status = Status.InProgress, ProjectId = 2 },
                new Task { Id = 4, Title = "Task B2", Description = "Beta Task 2", Deadline = DateTime.Now.AddDays(15), Status = Status.Cancelled, ProjectId = 2 }
            );

            modelBuilder.Entity<Employee>().HasData(
                new Employee { Id = 1, FullName = "John Doe", Age = 28, Position = Position.Middle },
                new Employee { Id = 2, FullName = "Jane Smith", Age = 35, Position = Position.Senior },
                new Employee { Id = 3, FullName = "Alice Johnson", Age = 24, Position = Position.Junior }
            );

            modelBuilder.Entity<Task>()
                .HasMany(t => t.Employees)
                .WithMany(e => e.Tasks)
                .UsingEntity(j => j
                    .ToTable("EmployeeTasks")
                    .HasData(
                        new { TasksId = 1, EmployeesId = 1 },
                        new { TasksId = 1, EmployeesId = 3 },
                        new { TasksId = 2, EmployeesId = 2 },
                        new { TasksId = 3, EmployeesId = 1 },
                        new { TasksId = 4, EmployeesId = 3 }
                    )
                );

            base.OnModelCreating(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EFCoreDB;Trusted_Connection=True;");
        }
    }
}
