using Microsoft.EntityFrameworkCore;
using Web.Models;

public class WebdevP3Context : DbContext
{
    public WebdevP3Context(DbContextOptions<WebdevP3Context> options) : base(options) { }

    public DbSet<Venue> Venues { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<EventType> EventTypes { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venue>().ToTable("Venue");
        modelBuilder.Entity<Event>().ToTable("Event");
        modelBuilder.Entity<Booking>().ToTable("Booking");
        modelBuilder.Entity<EventType>().ToTable("EventTypes");
    }
}