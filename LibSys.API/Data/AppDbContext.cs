using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LibSys.API.Models;

namespace LibSys.API.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Borrowing> Borrowings => Set<Borrowing>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Borrowing>(e =>
        {
            e.HasOne(b => b.User).WithMany(u => u.Borrowings).HasForeignKey(b => b.UserId);
            e.HasOne(b => b.Book).WithMany(bk => bk.Borrowings).HasForeignKey(b => b.BookId);
        });

        builder.Entity<Reservation>(e =>
        {
            e.HasOne(r => r.User).WithMany(u => u.Reservations).HasForeignKey(r => r.UserId);
            e.HasOne(r => r.Book).WithMany(bk => bk.Reservations).HasForeignKey(r => r.BookId);
        });

        builder.Entity<Notification>(e =>
        {
            e.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId);
        });
    }
}

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var db = services.GetRequiredService<AppDbContext>();

        // Seed Roles
        foreach (var role in new[] { "Admin", "Member" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed Admin user
        if (await userManager.FindByEmailAsync("admin@libsys.com") == null)
        {
            var admin = new AppUser
            {
                UserName = "admin@libsys.com",
                Email = "admin@libsys.com",
                FullName = "System Admin",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin@1234");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Seed sample books
        if (!db.Books.Any())
        {
            db.Books.AddRange(
                new Book { Title = "Clean Code", Author = "Robert C. Martin", ISBN = "9780132350884", Category = "Programming", TotalCopies = 3, AvailableCopies = 3, Description = "A handbook of agile software craftsmanship" },
                new Book { Title = "The Pragmatic Programmer", Author = "Andrew Hunt", ISBN = "9780135957059", Category = "Programming", TotalCopies = 2, AvailableCopies = 2, Description = "Your journey to mastery" },
                new Book { Title = "Design Patterns", Author = "Gang of Four", ISBN = "9780201633610", Category = "Programming", TotalCopies = 2, AvailableCopies = 2, Description = "Elements of Reusable Object-Oriented Software" },
                new Book { Title = "Introduction to Algorithms", Author = "CLRS", ISBN = "9780262046305", Category = "Computer Science", TotalCopies = 4, AvailableCopies = 4, Description = "Comprehensive algorithms textbook" },
                new Book { Title = "C# in Depth", Author = "Jon Skeet", ISBN = "9781617294532", Category = "Programming", TotalCopies = 3, AvailableCopies = 3, Description = "Master C# from basics to advanced" }
            );
            await db.SaveChangesAsync();
        }
    }
}
