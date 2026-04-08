using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext>
        options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(p =>
            {
                p.HasData(
                    new Product
                    {
                        Id = 1, Name = "Laptop", Price = 1299.99m,
                        Description = "High-performance laptop with 16GB RAM", Category = "Electronics"
                    },
                    new Product
                    {
                        Id = 2, Name = "Wireless Mouse", Price = 29.99m,
                        Description = "Ergonomic wireless mouse with precision tracking", Category = "Electronics"
                    },
                    new Product
                    {
                        Id = 3, Name = "Mechanical Keyboard", Price = 89.99m,
                        Description = "RGB mechanical keyboard with blue switches", Category = "Electronics"
                    },
                    new Product
                    {
                        Id = 4, Name = "Office Chair", Price = 249.99m,
                        Description = "Ergonomic office chair with lumbar support", Category = "Furniture"
                    },
                    new Product
                    {
                        Id = 5, Name = "Standing Desk", Price = 399.99m,
                        Description = "Adjustable height standing desk", Category = "Furniture"
                    },
                    new Product
                    {
                        Id = 6, Name = "Monitor 27\"", Price = 349.99m, Description = "4K UHD 27-inch monitor with HDR",
                        Category = "Electronics"
                    },
                    new Product
                    {
                        Id = 7, Name = "Webcam HD", Price = 79.99m, Description = "1080p HD webcam with auto-focus",
                        Category = "Electronics"
                    },
                    new Product
                    {
                        Id = 8, Name = "Desk Lamp", Price = 39.99m,
                        Description = "LED desk lamp with adjustable brightness", Category = "Office Supplies"
                    },
                    new Product
                    {
                        Id = 9, Name = "Notebook Set", Price = 14.99m, Description = "Set of 5 professional notebooks",
                        Category = "Office Supplies"
                    },
                    new Product
                    {
                        Id = 10, Name = "USB-C Hub", Price = 59.99m,
                        Description = "7-in-1 USB-C hub with HDMI and ethernet", Category = "Electronics"
                    }
                );
            }
        );
    }
}