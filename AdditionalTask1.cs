using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO.NET_HW_16_10
{
    class Program
    {
        static void Main()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                DatabaseService ds = new DatabaseService(db);

                ds.AddGuestOnEvent(1, 2, Roles.VIP);

                var guestsOnEvent = ds.GetGuestsByEvent(1);

                ds.ChangeGuestRole(1, 2, Roles.Organizer);

                var eventsForGuest = ds.GetEventsByGuest(3);

                ds.DeleteGuestFromEvent(2, 1);

                var eventsWhereGuestHaveRole = ds.GetEventByGuestRole(3, Roles.Organizer);

                var topGuest = ds.GetTop3Guests();

                foreach (var guest in topGuest)
                {
                    Console.WriteLine($"{guest.Fullname} | {guest.ParticipationCount}");
                }

                db.SaveChanges();

            }
        }
    }
    public class Guest
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public List<Event> Events { get; set; }
    }
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<Guest> Guests { get; set; }
    }
    public class GuestRoles
    {
        public int Id { get; set; }
        public Roles Role { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
        public int GuestId { get; set; }
        public Guest Guest { get; set; }
    }
    public class TopGuestViewModel
    {
        public int GuestId { get; set; }
        public string Fullname { get; set; }
        public int ParticipationCount { get; set; }
        public List<Event> Events { get; set; }
    }
    public enum Roles
    {
        Regular,
        Speaker,
        Organizer,
        Sponsor,
        VIP,
        Performer,
        Volunteer
    }
    public class DatabaseService
    {
        private readonly ApplicationContext db;
        public DatabaseService(ApplicationContext db)
        {
            this.db = db;
        }
        public void AddGuestOnEvent(int guestId, int eventId, Roles role)
        {
            var guest = db.Guests.Find(guestId);
            if (guest == null)
            {
                Console.WriteLine("Guest not found!");
                return;
            }
            var _event = db.Events.Include(e => e.Guests).FirstOrDefault(e => e.Id == eventId);
            if (_event == null)
            {
                Console.WriteLine("Event not found!");
                return;
            }

            var guestRole = new GuestRoles() { GuestId = guestId, EventId = eventId, Role = role };

            db.GuestRoles.Add(guestRole);
            db.SaveChanges();
        }
        public List<Guest> GetGuestsByEvent(int eventId)
        {
            var _event = db.Events.Include(e => e.Guests).FirstOrDefault(e => e.Id == eventId);
            if (_event == null)
            {
                Console.WriteLine("Event not found!");
                return new List<Guest>();
            }
            return _event.Guests.ToList();
        }
        public void ChangeGuestRole(int guestId, int eventId, Roles role)
        {
            var guest = db.GuestRoles.FirstOrDefault(g => g.GuestId == guestId && g.EventId == eventId);
            if (guest == null)
            {
                Console.WriteLine("Guest not found!");
                return;
            }

            guest.Role = role;
            db.SaveChanges();
        }
        public List<Event> GetEventsByGuest(int guestId)
        {
            var guest = db.Guests.Include(e => e.Events).FirstOrDefault(g => g.Id == guestId);
            if (guest == null)
            {
                Console.WriteLine("Event not found!");
                return new List<Event>();
            }
            return guest.Events.ToList();
        }
        public void DeleteGuestFromEvent(int guestId, int eventId)
        {
            var guest = db.GuestRoles.FirstOrDefault(g => g.GuestId == guestId && g.EventId == eventId);

            if (guest == null)
            {
                Console.WriteLine("Guest not found.");
                return;
            }

            db.GuestRoles.Remove(guest);
            db.SaveChanges();
        }
        public List<Event> GetEventByGuestRole(int guestId, Roles role)
        {
            var guestRoles = db.GuestRoles
            .Where(gr => gr.GuestId == guestId && gr.Role == role)
            .Include(gr => gr.Event)
            .ToList();

            if (!guestRoles.Any())
            {
                Console.WriteLine($"No events found for guest {guestId} with role {role}.");
                return new List<Event>();
            }

            return guestRoles.Select(gr => gr.Event).ToList();
        }
        public List<TopGuestViewModel> GetTop3Guests()
        {
            var topGuests = db.GuestRoles
                .GroupBy(gr => new { gr.GuestId, gr.Guest.Fullname })
                .Select(g => new TopGuestViewModel
                {
                    GuestId = g.Key.GuestId,
                    Fullname = g.Key.Fullname,
                    ParticipationCount = g.Count(),
                    Events = g.Select(gr => gr.Event).ToList()
                })
                .OrderByDescending(g => g.ParticipationCount)
                .Take(3)
                .ToList();

            return topGuests;
        }
    }
    public class ApplicationContext : DbContext
    {
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<GuestRoles> GuestRoles { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFCoreDB;Trusted_Connection=True;");

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Guest>()
                .HasMany(g => g.Events)
                .WithMany(e => e.Guests)
                .UsingEntity<GuestRoles>(
                r =>
                {
                    r.Property(r => r.Role);
                    r.HasKey(r => r.Id);
                    r.ToTable("GuestRole");
                });
            modelBuilder.Entity<Guest>().HasData(
                new Guest { Id = 1, Fullname = "John Doe", Age = 30, Email = "john.doe@example.com" },
                new Guest { Id = 2, Fullname = "Jane Smith", Age = 28, Email = "jane.smith@example.com" },
                new Guest { Id = 3, Fullname = "Michael Brown", Age = 40, Email = "michael.brown@example.com" }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event { Id = 1, Title = "Tech Conference", Description = "Annual tech conference", StartTime = DateTime.Parse("2024-10-10 09:00"), EndTime = DateTime.Parse("2024-10-10 17:00") },
                new Event { Id = 2, Title = "Music Festival", Description = "Outdoor music festival", StartTime = DateTime.Parse("2024-11-15 12:00"), EndTime = DateTime.Parse("2024-11-15 23:00") }
            );

            modelBuilder.Entity<GuestRoles>().HasData(
                new GuestRoles { Id = 1, Role = Roles.Speaker, GuestId = 1, EventId = 1 },
                new GuestRoles { Id = 2, Role = Roles.Regular, GuestId = 2, EventId = 1 },
                new GuestRoles { Id = 3, Role = Roles.Organizer, GuestId = 3, EventId = 2 },
                new GuestRoles { Id = 4, Role = Roles.VIP, GuestId = 1, EventId = 2 }
            );
            base.OnModelCreating(modelBuilder);
        }
    }
}
