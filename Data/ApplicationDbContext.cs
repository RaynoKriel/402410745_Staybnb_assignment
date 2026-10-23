using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using _402410745_Staybnb_assignment.Models;

namespace _402410745_Staybnb_assignment.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<HostProperty> HostProperties { get; set; }
    public DbSet<PropertyImage> PropertyImages { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<HostApplication> HostApplications { get; set; }
    public DbSet<CheckInProcess> CheckInProcesses { get; set; }
    public DbSet<GuestCheckIn> GuestCheckIns { get; set; }
    public DbSet<GuestDocument> GuestDocuments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // One-to-one relationships (the table holding the foreign key is the "dependent")
        builder.Entity<Booking>().HasOne(b => b.Payment).WithOne(p => p.Booking).HasForeignKey<Payment>(p => p.BookingId);
        builder.Entity<Booking>().HasOne(b => b.GuestCheckIn).WithOne(g => g.Booking).HasForeignKey<GuestCheckIn>(g => g.BookingId);
        builder.Entity<HostProperty>().HasOne(p => p.CheckInProcess).WithOne(c => c.Property).HasForeignKey<CheckInProcess>(c => c.PropertyId);

        // SQL Server rejects tables with several cascade-delete paths.
        // The app never deletes rows, so every one of our own foreign keys is set to Restrict.
        foreach (var fk in builder.Model.GetEntityTypes()
                     .Where(e => e.ClrType.Namespace == "_402410745_Staybnb_assignment.Models")
                     .SelectMany(e => e.GetForeignKeys()))
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
